namespace Smbd.Csr;

using Shared.Http;

public class App : HttpServer
{
    public App()
    {
    }

    public override void Init()
    {
        router.Use(HttpUtils.StructuredLogging);
        router.Use(HttpUtils.CentralizedErrorHandling);
        router.Use(HttpUtils.AddResponseCorsHeaders);
        router.Use(HttpUtils.DefaultResponse);
        router.Use(HttpUtils.ParseRequestUrl);
        router.Use(HttpUtils.ParseRequestQueryString);
        router.Use(HttpUtils.ServeStaticFiles);
        router.UseSimpleRouteMatching();

        router.MapGet("/", async (req, res, props, next) =>
        { 
            res.Redirect("/index.html"); 
            await next(); 
        });

        router.MapGet("/movies", async (req, res, props, next) =>
        { 
            res.Redirect("/movies/index.html"); 
            await next(); 
        });
    }
}
