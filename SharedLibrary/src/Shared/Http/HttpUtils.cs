namespace Shared.Http;

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using System.Xml.Linq;
using Shared.Config;

public static class HttpUtils
{
    // ---------------------------------------------------------
    // LOGGING ESTRUCTURADO
    // Registra información clave de cada request/response.
    // ---------------------------------------------------------
    public static async Task StructuredLogging(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        var requestId = props["req.id"]?.ToString()
            ?? Guid.NewGuid().ToString("n").Substring(0, 12);

        var startUtc = DateTime.UtcNow;
        var method = req.HttpMethod ?? "UNKNOWN";
        var url = req.Url?.OriginalString ?? req.Url?.ToString() ?? "";
        var remote = req.RemoteEndPoint?.ToString() ?? "unknown";

        res.Headers["X-Request-Id"] = requestId;

        try
        {
            await next();
        }
        finally
        {
            var duration = (DateTime.UtcNow - startUtc).TotalNanoseconds;

            var record = new
            {
                timestamp = startUtc.ToString("o"),
                requestId,
                method,
                url,
                remote,
                statusCode = res.StatusCode,
                contentType = res.ContentType,
                contentLength = res.ContentLength64,
                duration
            };

            Console.WriteLine(JsonSerializer.Serialize(record, JsonSerializerOptions.Web));
        }
    }

    // ---------------------------------------------------------
    // MANEJO CENTRALIZADO DE ERRORES
    // Envuelve el siguiente middleware en try/catch.
    // ---------------------------------------------------------
    public static async Task CentralizedErrorHandling(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        try
        {
            await next();
        }
        catch (Exception e)
        {
            int code = (int)HttpStatusCode.InternalServerError;
            string message =
                Environment.GetEnvironmentVariable("DEPLOYMENT_MODE") == "production"
                    ? "An unexpected error occurred."
                    : e.ToString();

            await SendResponse(req, res, props, code, message, "text/plain");
        }
    }

    // ---------------------------------------------------------
    // RESPUESTA POR DEFECTO (404)
    // Si nadie escribió la respuesta, manda 404.
    // ---------------------------------------------------------
    public static async Task DefaultResponse(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        await next();

        if (res.StatusCode == HttpRouter.RESPONSE_NOT_SENT)
        {
            res.StatusCode = (int)HttpStatusCode.NotFound;
            res.Close();
        }
    }

    // ---------------------------------------------------------
    // SERVICIO DE ARCHIVOS ESTÁTICOS
    // Sirve archivos desde root.dir usando la URL como path.
    // ---------------------------------------------------------
    public static async Task ServeStaticFiles(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        string rootDir = Configuration.Get("wwwroot.dir", Directory.GetCurrentDirectory())!;
        string urlPath = req.Url!.AbsolutePath.TrimStart('/');
        string filePath = Path.Combine(rootDir, urlPath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(filePath))
        {
            using var fs = File.OpenRead(filePath);
            res.StatusCode = (int)HttpStatusCode.OK;
            res.ContentType = GetMimeType(filePath);
            res.ContentLength64 = fs.Length;
            await fs.CopyToAsync(res.OutputStream);
            res.Close();
            return;
        }

        await next();
    }

    private static string GetMimeType(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLowerInvariant();

        return ext switch
        {
            ".html" or ".htm" => "text/html; charset=utf-8",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".txt" => "text/plain; charset=utf-8",
            _ => "application/octet-stream"
        };
    }

    // ---------------------------------------------------------
    // CORS – AGREGA HEADERS A LA RESPUESTA
    // Soporta modo dev (todo permitido) y producción (lista blanca).
    // ---------------------------------------------------------
    public static async Task AddResponseCorsHeaders(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        bool isProductionMode =
            Environment.GetEnvironmentVariable("DEPLOYMENT_MODE") == "Production";

        string? origin = req.Headers["Origin"];

        if (!string.IsNullOrEmpty(origin))
        {
            if (!isProductionMode)
            {
                // Desarrollo: permitir todo
                res.AddHeader("Access-Control-Allow-Origin", origin);
                res.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");
                res.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                res.AddHeader("Access-Control-Allow-Credentials", "true");
            }
            else
            {
                string[] allowedOrigins =
                    Configuration.Get("allowed.origins", string.Empty)!
                        .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                {
                    res.AddHeader("Access-Control-Allow-Origin", origin);
                    res.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");
                    res.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    res.AddHeader("Access-Control-Allow-Credentials", "true");
                }
            }
        }

        if (req.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            res.StatusCode = (int)HttpStatusCode.NoContent;
            res.OutputStream.Close();
            return;
        }

        await next();
    }

    // ---------------------------------------------------------
    // PARSEO DE URL COMPLETA A PARTES (scheme, host, path, query…)
    // ---------------------------------------------------------
    public static NameValueCollection ParseUrl(string url)
    {
        int i = -1;

        var (scheme, aqpf) = (i = url.IndexOf("://")) >= 0
            ? (url.Substring(0, i), url.Substring(i + 3))
            : ("", url);

        var (auth, pqf) = (i = aqpf.IndexOf("/")) >= 0
            ? (aqpf.Substring(0, i), aqpf.Substring(i))
            : (aqpf, "");

        var (up, hp) = (i = auth.IndexOf("@")) >= 0
            ? (auth.Substring(0, i), auth.Substring(i + 1))
            : ("", auth);

        var (user, pass) = (i = up.IndexOf(":")) >= 0
            ? (up.Substring(0, i), up.Substring(i + 1))
            : (up, "");

        var (host, port) = (i = hp.IndexOf(":")) >= 0
            ? (hp.Substring(0, i), hp.Substring(i + 1))
            : (hp, "");

        var (pq, fragment) = (i = pqf.IndexOf("#")) >= 0
            ? (pqf.Substring(0, i), pqf.Substring(i + 1))
            : (pqf, "");

        var (path, query) = (i = pq.IndexOf("?")) >= 0
            ? (pq.Substring(0, i), pq.Substring(i + 1))
            : (pq, "");

        var parts = new NameValueCollection
        {
            ["scheme"] = scheme,
            ["auth"] = auth,
            ["user"] = user,
            ["pass"] = pass,
            ["host"] = host,
            ["port"] = port,
            ["path"] = path,
            ["query"] = query,
            ["fragment"] = fragment
        };

        return parts;
    }

    public static async Task ParseRequestUrl(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        props["req.url"] = ParseUrl(req.Url!.OriginalString);
        await next();
    }

    // ---------------------------------------------------------
    // QUERY STRING – convierte ?a=1&b=2 en NameValueCollection
    // ---------------------------------------------------------
    public static NameValueCollection ParseQueryString(
        string text,
        string duplicateSeparator = ",")
    {
        if (text.StartsWith("?"))
            text = text.Substring(1);

        return ParseFormData(text, duplicateSeparator);
    }

    public static async Task ParseRequestQueryString(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        var url = (NameValueCollection)props["req.url"];
        props["req.query"] = ParseQueryString(url["query"] ?? req.Url!.Query);
        await next();
    }

    // ---------------------------------------------------------
    // FORM DATA – a=b&c=d => NameValueCollection
    // ---------------------------------------------------------
    public static NameValueCollection ParseFormData(
        string text,
        string duplicateSeparator = ",")
    {
        var result = new NameValueCollection();
        var pairs = text.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var kv = pair.Split('=', 2, StringSplitOptions.None);
            var key = HttpUtility.UrlDecode(kv[0]) ?? "";
            var value = kv.Length > 1 ? HttpUtility.UrlDecode(kv[1]) ?? "" : string.Empty;

            var oldValue = result[key];
            result[key] = oldValue == null ? value : oldValue + duplicateSeparator + value;
        }

        return result;
    }

    public static async Task ReadRequestBodyAsForm(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        using StreamReader sr = new(req.InputStream, Encoding.UTF8);
        string formData = await sr.ReadToEndAsync();
        props["req.form"] = ParseFormData(formData);
        await next();
    }

    // ---------------------------------------------------------
    // LECTURA DEL BODY EN DIFERENTES FORMATOS
    // ---------------------------------------------------------
    public static async Task ReadRequestBodyAsBlob(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        using var ms = new MemoryStream();
        await req.InputStream.CopyToAsync(ms);
        props["req.blob"] = ms.ToArray();
        await next();
    }

    public static async Task ReadRequestBodyAsText(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        var encoding = req.ContentEncoding ?? Encoding.UTF8;
        using StreamReader sr = new(req.InputStream, encoding);
        props["req.text"] = await sr.ReadToEndAsync();
        await next();
    }

    public static async Task ReadRequestBodyAsJson(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        props["req.json"] = (await JsonNode.ParseAsync(req.InputStream))!.AsObject();
        await next();
    }

    public static async Task ReadRequestBodyAsXml(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        props["req.xml"] = await XDocument.LoadAsync(
            req.InputStream,
            LoadOptions.None,
            CancellationToken.None);
        await next();
    }

    // ---------------------------------------------------------
    // DETECCIÓN DEL CONTENT-TYPE SEGÚN EL TEXTO
    // ---------------------------------------------------------
    public static string DetectContentType(string text)
    {
        string s = text.TrimStart();

        if (s.StartsWith("{") || s.StartsWith("["))
        {
            return "application/json";
        }
        else if (s.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase)
                 || s.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
        {
            return "text/html";
        }
        else if (s.StartsWith("<", StringComparison.Ordinal))
        {
            return "application/xml";
        }
        else
        {
            return "text/plain";
        }
    }

    // ---------------------------------------------------------
    // HELPERS DE RESPUESTA – 200 OK
    // ---------------------------------------------------------
    public static Task SendOkResponse(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props)
    {
        return SendOkResponse(req, res, props, string.Empty, "text/plain");
    }

    public static Task SendOkResponse(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        string content)
    {
        return SendOkResponse(req, res, props, content, DetectContentType(content));
    }

    public static Task SendOkResponse(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        string content,
        string contentType)
    {
        return SendResponse(req, res, props, (int)HttpStatusCode.OK, content, contentType);
    }

    // ---------------------------------------------------------
    // HELPERS DE RESPUESTA – 404 NOT FOUND
    // ---------------------------------------------------------
    public static Task SendNotFoundResponse(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props)
    {
        return SendNotFoundResponse(req, res, props, string.Empty, "text/plain");
    }

    public static Task SendNotFoundResponse(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        string content)
    {
        return SendNotFoundResponse(req, res, props, content, DetectContentType(content));
    }

    public static Task SendNotFoundResponse(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        string content,
        string contentType)
    {
        return SendResponse(req, res, props, (int)HttpStatusCode.NotFound, content, contentType);
    }

    // ---------------------------------------------------------
    // HELPERS GENERALES – CUALQUIER STATUS CODE
    // ---------------------------------------------------------
    public static Task SendResponse(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        int statusCode,
        string content)
    {
        return SendResponse(req, res, props, statusCode, content, DetectContentType(content));
    }

    public static Task SendResponse(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        int statusCode,
        string content,
        string contentType)
    {
        return SendResponse(req, res, props, statusCode, Encoding.UTF8.GetBytes(content), contentType);
    }

    public static async Task SendResponse(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        int statusCode,
        byte[] content,
        string contentType)
    {
        res.StatusCode = statusCode;
        res.ContentEncoding = Encoding.UTF8;
        res.ContentType = contentType;
        res.ContentLength64 = content.LongLength;
        await res.OutputStream.WriteAsync(content, 0, content.Length);
        res.Close();
    }

    public static async Task SendResultResponse<T>(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Result<T> result)
    {
        if (result.IsError)
        {
            res.Headers["Cache-Control"] = "no-store";
            await HttpUtils.SendResponse(
                req, res, props, result.StatusCode, result.Error!.ToString()!);
        }
        else
        {
            await HttpUtils.SendResponse(
                req, res, props, result.StatusCode, result.Payload!.ToString()!);
        }
    }

    public static async Task SendPagedResultResponse<T>(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Result<PagedResult<T>> result,
        int page,
        int size)
    {
        if (result.IsError)
        {
            res.Headers["Cache-Control"] = "no-store";
            await HttpUtils.SendResponse(
                req, res, props, result.StatusCode, result.Error!.ToString()!);
        }
        else
        {
            var pagedResult = result.Payload!;
            HttpUtils.AddPaginationHeaders(req, res, props, pagedResult, page, size);
            await HttpUtils.SendResponse(
                req, res, props, result.StatusCode, result.Payload!.ToString()!);
        }
    }

    public static void AddPaginationHeaders<T>(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        PagedResult<T> pagedResult,
        int page,
        int size)
    {
        // Base URL reconstruida desde el request actual
        var baseUrl =
            $"{req.Url!.Scheme}://{req.Url.Authority}{req.Url.AbsolutePath}";

        // Total de páginas (mínimo 1)
        int totalPages =
            Math.Max(1, (int)Math.Ceiling((double)pagedResult.TotalCount / size));

        // Enlaces de navegación (RFC 5988)
        string self = $"{baseUrl}?page={page}&size={size}";
        string? first = page == 1 ? null : $"{baseUrl}?page=1&size={size}";
        string? last = page == totalPages ? null : $"{baseUrl}?page={totalPages}&size={size}";
        string? prev = page > 1 ? $"{baseUrl}?page={page - 1}&size={size}" : null;
        string? next = page < totalPages ? $"{baseUrl}?page={page + 1}&size={size}" : null;

        // Headers estándar de paginación
        res.Headers["Content-Type"] = "application/json; charset=utf-8";
        res.Headers["X-Total-Count"] = pagedResult.TotalCount.ToString();
        res.Headers["X-Page"] = page.ToString();
        res.Headers["X-Page-Size"] = size.ToString();
        res.Headers["X-Total-Pages"] = totalPages.ToString();

        // Construcción del header "Link" (descubrimiento de navegación)
        var linkParts = new List<string>();

        if (prev != null) linkParts.Add($"<{prev}>; rel=\"prev\"");
        if (next != null) linkParts.Add($"<{next}>; rel=\"next\"");
        if (first != null) linkParts.Add($"<{first}>; rel=\"first\"");
        if (last != null) linkParts.Add($"<{last}>; rel=\"last\"");

        if (linkParts.Count > 0)
        {
            res.Headers["Link"] = string.Join(", ", linkParts);
        }
    }
}
