namespace Shared.Http;

using Shared.Config;
using System.Net;

public abstract class HttpServer
{
    // Router principal que manejará todas las rutas/middlewares.
    protected HttpRouter router;

    // HttpListener nativo de .NET que escucha las peticiones HTTP.
    protected HttpListener server;

    // ---------------------------------------------------------
    // Constructor:
    //  - Crea el router
    //  - Llama a Init() para que la subclase configure rutas/middleware
    //  - Lee HOST y PORT de la configuración
    //  - Configura el HttpListener y muestra la URL de inicio
    // ---------------------------------------------------------
    public HttpServer()
    {
        router = new HttpRouter();

        // La subclase define aquí su pipeline (Use, Map, UseRouter, etc.)
        Init();

        string host = Configuration.Get<string>("HOST", "http://127.0.0.1");
        string port = Configuration.Get<string>("PORT", "5000");
        string authority = $"{host}:{port}/";

        server = new HttpListener();
        server.Prefixes.Add(authority);

        Console.WriteLine("Server started at " + authority);
    }

    // ---------------------------------------------------------
    // Cada implementación concreta debe configurar:
    //  - Middlewares globales
    //  - Routers anidados
    //  - Rutas y controladores
    // ---------------------------------------------------------
    public abstract void Init();

    // ---------------------------------------------------------
    // Inicia el servidor:
    //  - Empieza a escuchar
    //  - Acepta requests en bucle
    //  - Por cada request, delega al router de manera asíncrona
    // ---------------------------------------------------------
    public async Task Start()
    {
        server.Start();

        while (server.IsListening)
        {
            HttpListenerContext ctx = await server.GetContextAsync();
            _ = router.HandleContextAsync(ctx); // Se maneja en background
        }
    }

    // ---------------------------------------------------------
    // Detiene el servidor limpiamente y libera recursos.
    // ---------------------------------------------------------
    public void Stop()
    {
        if (server.IsListening)
        {
            server.Stop();
            server.Close();
            Console.WriteLine("Server stopped.");
        }
    }
}
