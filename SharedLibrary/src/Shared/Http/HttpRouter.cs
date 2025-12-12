namespace Shared.Http;

using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Web;

public class HttpRouter
{
    public const int RESPONSE_NOT_SENT = 777;

    private static ulong requestId = 0;
    private string basePath;
    private List<HttpMiddleware> middlewares;
    private List<(string method, string path, HttpMiddleware[] mws)> routes;

    public HttpRouter()
    {
        basePath = string.Empty;
        middlewares = new();
        routes = new();
    }

    // ---------------------------------------------------------
    // GLOBAL MIDDLEWARE (se ejecuta para cada request)
    // Permite registrar una o varias funciones middleware.
    // ---------------------------------------------------------
    public HttpRouter Use(params HttpMiddleware[] mws)
    {
        middlewares.AddRange(mws);
        return this;
    }

    // ---------------------------------------------------------
    // ROUTE MAPPING
    // Registra rutas específicas para un método HTTP.
    // Cada ruta asocia GET/POST/PUT/DELETE con middlewares.
    // ---------------------------------------------------------
    public HttpRouter Map(string method, string path, params HttpMiddleware[] mws)
    {
        routes.Add((method.ToUpperInvariant(), path, mws));
        return this;
    }

    public HttpRouter MapGet(string path, params HttpMiddleware[] mws) => Map("GET", path, mws);
    public HttpRouter MapPost(string path, params HttpMiddleware[] mws) => Map("POST", path, mws);
    public HttpRouter MapPut(string path, params HttpMiddleware[] mws) => Map("PUT", path, mws);
    public HttpRouter MapDelete(string path, params HttpMiddleware[] mws) => Map("DELETE", path, mws);

    // ---------------------------------------------------------
    // FRONT CONTROLLER
    // Este método recibe cada request del HttpServer.
    //       - Inicializa props
    //       - Marca la respuesta como "no enviada"
    //       - Ejecuta el middleware global
    // ---------------------------------------------------------
    public async Task HandleContextAsync(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        var res = ctx.Response;
        var props = new Hashtable();

        res.StatusCode = RESPONSE_NOT_SENT;
        props["req.id"] = ++requestId;

        try
        {
            await HandleAsync(req, res, props, () => Task.CompletedTask);
        }
        finally
        {
            if (res.StatusCode == RESPONSE_NOT_SENT)
            {
                res.StatusCode = (int)HttpStatusCode.NotImplemented;
            }

            res.Close();
        }
    }

    // ---------------------------------------------------------
    // ROUTER COMO MIDDLEWARE
    // Crea el pipeline de middlewares globales y lo ejecuta.
    // Incluye UseRouter (routers anidados).
    // ---------------------------------------------------------
    private async Task HandleAsync(HttpListenerRequest req, HttpListenerResponse res, Hashtable props, Func<Task> next)
    {
        Func<Task> pipeline = GenerateMiddlewarePipeline(req, res, props, middlewares);
        await pipeline();
        await next();
    }

    // ---------------------------------------------------------
    // ROUTERS ANIDADOS (Sub-rutas)
    // Permite hacer:
    //    apiRouter.UseRouter("/movies", moviesRouter)
    // ---------------------------------------------------------
    public HttpRouter UseRouter(string path, HttpRouter router)
    {
        router.basePath = this.basePath + path;
        return Use(router.HandleAsync);
    }

    // ---------------------------------------------------------
    // GENERADOR DEL PIPELINE DE MIDDLEWARES
    // Crea una función que ejecuta los middlewares en orden.
    // next() hace avanzar al siguiente middleware del pipeline.
    // ---------------------------------------------------------
    private Func<Task> GenerateMiddlewarePipeline(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        List<HttpMiddleware> mws)
    {
        int index = -1;

        Func<Task> next = () => Task.CompletedTask;

        next = async () =>
        {
            index++;

            if (index < mws.Count && res.StatusCode == RESPONSE_NOT_SENT)
            {
                await mws[index](req, res, props, next);
            }
        };

        return next;
    }

    // ---------------------------------------------------------
    // ROUTE MATCHING – instalar middlewares para rutas
    // ---------------------------------------------------------
    public HttpRouter UseSimpleRouteMatching()
    {
        return Use(SimpleRouteMatching);
    }

    public HttpRouter UseParameterizedRouteMatching()
    {
        return Use(ParametrizedRouteMatching);
    }

    // ---------------------------------------------------------
    // RUTA ESTÁTICA (SIN PARÁMETROS)
    // Compara método + path exacto.
    // ---------------------------------------------------------
    private async Task SimpleRouteMatching(HttpListenerRequest req, HttpListenerResponse res, Hashtable props, Func<Task> next)
    {
        foreach (var (method, path, mws) in routes)
        {
            if (req.HttpMethod == method &&
                string.Equals(req.Url!.AbsolutePath, basePath + path))
            {
                // Construir pipeline de la ruta
                var routePipeline =
                    GenerateMiddlewarePipeline(req, res, props, mws.ToList());

                await routePipeline();
                break; // corta pipeline global
            }
        }

        await next();
    }

    // ---------------------------------------------------------
    // RUTA PARAMETRIZADA (p.ej. /actors/:id/movies/:mid)
    // Permite capturar parámetros desde el path.
    // ---------------------------------------------------------
    private async Task ParametrizedRouteMatching(HttpListenerRequest req, HttpListenerResponse res, Hashtable props, Func<Task> next)
    {
        foreach (var (method, path, mws) in routes)
        {
            NameValueCollection? parameters;

            if (req.HttpMethod == method &&
                (parameters = ParseUrlParams(req.Url!.AbsolutePath, basePath + path)) != null)
            {
                props["req.params"] = parameters;

                var routePipeline =
                    GenerateMiddlewarePipeline(req, res, props, mws.ToList());

                await routePipeline();
                break;
            }
        }

        await next();
    }

    // ---------------------------------------------------------
    // PARSEADOR DE RUTAS PARAMETRIZADAS
    // Convierte rutas del tipo:
    //     /actors/:id/movies/:mid
    // a parámetros:
    //     { id = 4, mid = 2 }
    // ---------------------------------------------------------
    public static NameValueCollection? ParseUrlParams(string uPath, string rPath)
    {
        string[] uParts = uPath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        string[] rParts = rPath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (uParts.Length != rParts.Length)
            return null;

        var parameters = new NameValueCollection();

        for (int i = 0; i < rParts.Length; i++)
        {
            string uPart = uParts[i];
            string rPart = rParts[i];

            // Parámetro :id
            if (rPart.StartsWith(":"))
            {
                string paramName = rPart.Substring(1);
                parameters[paramName] = HttpUtility.UrlDecode(uPart);
            }
            // Segmento fijo
            else if (uPart != rPart)
            {
                return null;
            }
        }

        return parameters;
    }
}
