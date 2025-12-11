namespace Smdb.Core.Movies;

using Shared.Http;
using System.Net;

public class DefaultMovieService : IMovieService
{
    private IMovieRepository movieRepository;

    public DefaultMovieService(IMovieRepository movieRepository)
    {
        this.movieRepository = movieRepository;
    }

    // Lee películas con validación básica de paginación
    public async Task<Result<PagedResult<Movie>>> ReadMovies(int page, int size)
    {
        if(page < 1)
        {
            return new Result<PagedResult<Movie>>(
                new Exception("Page must be >= 1."),
                (int) HttpStatusCode.BadRequest
            );
        }

        if(size < 1)
        {
            return new Result<PagedResult<Movie>>(
                new Exception("Page size must be >= 1."),
                (int) HttpStatusCode.BadRequest
            );
        }

        var pagedResult = await movieRepository.ReadMovies(page, size);

        var result = pagedResult == null
            ? new Result<PagedResult<Movie>>(
                new Exception($"Could not read movies from page {page} and size {size}."),
                (int) HttpStatusCode.NotFound
            )
            : new Result<PagedResult<Movie>>(pagedResult, (int) HttpStatusCode.OK);

        return result;
    }

    // Crea una película con validación
    public async Task<Result<Movie>> CreateMovie(Movie newMovie)
    {
        var validationResult = ValidateMovie(newMovie);
        if(validationResult != null) return validationResult;

        var movie = await movieRepository.CreateMovie(newMovie);

        var result = movie == null
            ? new Result<Movie>(
                new Exception($"Could not create movie {newMovie}."),
                (int) HttpStatusCode.NotFound
            )
            : new Result<Movie>(movie, (int) HttpStatusCode.Created);

        return result;
    }

    // Lee película por ID
    public async Task<Result<Movie>> ReadMovie(int id)
    {
        var movie = await movieRepository.ReadMovie(id);

        var result = movie == null
            ? new Result<Movie>(
                new Exception($"Could not read movie with id {id}."),
                (int) HttpStatusCode.NotFound
            )
            : new Result<Movie>(movie, (int) HttpStatusCode.OK);

        return result;
    }

    // Actualiza película con validación
    public async Task<Result<Movie>> UpdateMovie(int id, Movie newData)
    {
        var validationResult = ValidateMovie(newData);
        if(validationResult != null) return validationResult;

        var movie = await movieRepository.UpdateMovie(id, newData);

        var result = movie == null
            ? new Result<Movie>(
                new Exception($"Could not update movie {newData} with id {id}."),
                (int) HttpStatusCode.NotFound
            )
            : new Result<Movie>(movie, (int) HttpStatusCode.OK);

        return result;
    }

    // Elimina película por ID
    public async Task<Result<Movie>> DeleteMovie(int id)
    {
        var movie = await movieRepository.DeleteMovie(id);

        var result = movie == null
            ? new Result<Movie>(
                new Exception($"Could not delete movie with id {id}."),
                (int) HttpStatusCode.NotFound
            )
            : new Result<Movie>(movie, (int) HttpStatusCode.OK);

        return result;
    }

    // Validación de payload
    private static Result<Movie>? ValidateMovie(Movie? movieData)
    {
        if(movieData == null)
        {
            return new Result<Movie>(
                new Exception("Movie payload is required."),
                (int) HttpStatusCode.BadRequest
            );
        }

        if(string.IsNullOrWhiteSpace(movieData.Title))
        {
            return new Result<Movie>(
                new Exception("Title is required and cannot be empty."),
                (int) HttpStatusCode.BadRequest
            );
        }

        if(movieData.Title.Length > 256)
        {
            return new Result<Movie>(
                new Exception("Title cannot be longer than 256 characters."),
                (int) HttpStatusCode.BadRequest
            );
        }

        if(movieData.Year < 1888 || movieData.Year > DateTime.UtcNow.Year)
        {
            return new Result<Movie>(
                new Exception($"Year must be between 1888 and {DateTime.UtcNow.Year}."),
                (int) HttpStatusCode.BadRequest
            );
        }

        return null;
    }
}
