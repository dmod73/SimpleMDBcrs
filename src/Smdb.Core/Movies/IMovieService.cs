namespace Smdb.Core.Movies;
using Shared.Http;
public interface IMovieService
{
public Task<Result<PagedResult<Movie>>> ReadMovies(int page, int size);
public Task<Result<Movie>> CreateMovie(Movie movie);
public Task<Result<Movie>> ReadMovie(int id);
public Task<Result<Movie>> UpdateMovie(int id, Movie newData);
public Task<Result<Movie>> DeleteMovie(int id);
}