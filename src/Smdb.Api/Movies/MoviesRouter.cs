namespace Smdb.Api.Movies;

using Shared.Http;
public class MoviesRouter : HttpRouter
{
    public MoviesRouter(MoviesController moviesController)
    {
        UseParameterizedRouteMatching();
        MapGet("/", moviesController.ReadMovies);
        MapPost("/", HttpUtils.ReadRequestBodyAsText, moviesController.CreateMovie);
        MapGet("/:id", moviesController.ReadMovie);
        MapPut("/:id", HttpUtils.ReadRequestBodyAsText,
        moviesController.UpdateMovie);
        MapDelete("/:id", moviesController.DeleteMovie);
    }
}
