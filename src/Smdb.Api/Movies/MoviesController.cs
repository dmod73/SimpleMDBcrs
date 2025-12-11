namespace Smdb.Api.Movies;

using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Shared.Http;
using Smdb.Core.Movies;

public class MoviesController
{
    private IMovieService movieService;

    public MoviesController(IMovieService movieService)
    {
        this.movieService = movieService;
    }

    // -------------------------------------------
    // GET /api/v1/movies?page=&size=
    // -------------------------------------------
    public async Task ReadMovies(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        int page = int.TryParse(req.QueryString["page"], out int p) ? p : 1;
        int size = int.TryParse(req.QueryString["size"], out int s) ? s : 9;

        var result = await movieService.ReadMovies(page, size);

        await JsonUtils.SendPagedResultResponse(req, res, props, result, page, size);

        await next();
    }

    // -------------------------------------------
    // POST /api/v1/movies
    // -------------------------------------------
    public async Task CreateMovie(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        var text = (string)props["req.text"]!;
        var movie = JsonSerializer.Deserialize<Movie>(text, JsonSerializerOptions.Web);

        var result = await movieService.CreateMovie(movie!);

        await JsonUtils.SendResultResponse(req, res, props, result);

        await next();
    }

    // -------------------------------------------
    // GET /api/v1/movies/:id
    // -------------------------------------------
    public async Task ReadMovie(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        var uParams = (NameValueCollection) props["req.params"]!;
        int id = int.TryParse(uParams["id"], out int i) ? i : -1;

        var result = await movieService.ReadMovie(id);

        await JsonUtils.SendResultResponse(req, res, props, result);

        await next();
    }

    // -------------------------------------------
    // PUT /api/v1/movies/:id
    // -------------------------------------------
    public async Task UpdateMovie(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        var uParams = (NameValueCollection) props["req.params"]!;
        int id = int.TryParse(uParams["id"], out int i) ? i : -1;

        var text = (string)props["req.text"]!;
        var movie = JsonSerializer.Deserialize<Movie>(text, JsonSerializerOptions.Web);

        var result = await movieService.UpdateMovie(id, movie!);

        await JsonUtils.SendResultResponse(req, res, props, result);

        await next();
    }

    // -------------------------------------------
    // DELETE /api/v1/movies/:id
    // -------------------------------------------
    public async Task DeleteMovie(
        HttpListenerRequest req,
        HttpListenerResponse res,
        Hashtable props,
        Func<Task> next)
    {
        var uParams = (NameValueCollection) props["req.params"]!;
        int id = int.TryParse(uParams["id"], out int i) ? i : -1;

        var result = await movieService.DeleteMovie(id);

        await JsonUtils.SendResultResponse(req, res, props, result);

        await next();
    }
}
