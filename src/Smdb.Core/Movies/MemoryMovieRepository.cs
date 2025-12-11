namespace Smbd.Core.Movies;

using Shared.Http;
using Smbd.Core.Db;

public class MemoryMovieRepository : IMovieRepository
{
    private MemoryDatabase db;

    public MemoryMovieRepository(MemoryDatabase db)
    {
        this.db = db;
    }

    public async Task<PagedResult<Movie>?> ReadMovies(int page, int size)
    {
        int totalCount = db.Movies.Count;
        int start = Math.Clamp((page - 1) * size, 0, totalCount);
        int length = Math.Clamp(size, 0, totalCount - start);
        var values = db.Movies.Slice(start, length);
        var result = new PagedResult<Movie>(totalCount, values);

        return await Task.FromResult(result);
    }

    public async Task<Movie?> CreateMovie(Movie newMovie)
    {
        newMovie.Id = db.NextMovieId();
        db.Movies.Add(newMovie);

        return await Task.FromResult(newMovie);
    }

    public async Task<Movie?> ReadMovie(int id)
    {
        Movie? result = db.Movies.FirstOrDefault(m => m.Id == id);
        return await Task.FromResult(result);
    }

    public async Task<Movie?> UpdateMovie(int id, Movie newData)
    {
        Movie? result = db.Movies.FirstOrDefault(m => m.Id == id);

        if (result != null)
        {
            result.Title = newData.Title;
            result.Year = newData.Year;
            result.Description = newData.Description;
        }

        return await Task.FromResult(result);
    }

    public async Task<Movie?> DeleteMovie(int id)
    {
        Movie? result = db.Movies.FirstOrDefault(m => m.Id == id);

        if (result != null)
        {
            db.Movies.Remove(result);
        }

        return await Task.FromResult(result);
    }
}
