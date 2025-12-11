namespace Smdb.Core.Test.Movies;

using Smdb.Core.Movies;
using Smdb.Core.Db;
using Shared.Http;

public class MemoryMovieRepositoryTests
{
    [Fact]
    public async Task ReadMovies_FirstPage_ReturnsCorrectMovies()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);

        // Act
        var result = await repo.ReadMovies(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.TotalCount);
        Assert.Equal(10, result.Values.Count);
    }

    [Fact]
    public async Task ReadMovies_SecondPage_ReturnsCorrectMovies()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);

        // Act
        var result = await repo.ReadMovies(2, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Values.Count);
        Assert.Equal(11, result.Values[0].Id); // Second page starts at movie 11
    }

    [Fact]
    public async Task ReadMovies_LastPage_ReturnsRemainingMovies()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);

        // Act
        var result = await repo.ReadMovies(5, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Values.Count);
    }

    [Fact]
    public async Task ReadMovies_PageSizeLargerThanTotal_ReturnsAllMovies()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);

        // Act
        var result = await repo.ReadMovies(1, 100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.Values.Count);
    }

    [Fact]
    public async Task ReadMovies_PageBeyondRange_ReturnsEmpty()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);

        // Act
        var result = await repo.ReadMovies(100, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Values);
    }

    [Fact]
    public async Task CreateMovie_AddsNewMovie()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);
        var newMovie = new Movie(0, "New Movie", 2025, "Test Description");

        // Act
        var result = await repo.CreateMovie(newMovie);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(51, result.Id); // Next available ID
        Assert.Equal("New Movie", result.Title);
        Assert.Contains(db.Movies, m => m.Id == 51);
    }

    [Fact]
    public async Task CreateMovie_AssignsIncrementalId()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);

        // Act
        var movie1 = await repo.CreateMovie(new Movie(0, "Movie 1", 2025, "Desc 1"));
        var movie2 = await repo.CreateMovie(new Movie(0, "Movie 2", 2025, "Desc 2"));

        // Assert
        Assert.Equal(51, movie1!.Id);
        Assert.Equal(52, movie2!.Id);
    }

    [Fact]
    public async Task ReadMovie_ExistingId_ReturnsMovie()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);

        // Act
        var result = await repo.ReadMovie(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("The Godfather", result.Title);
    }

    [Fact]
    public async Task ReadMovie_NonExistingId_ReturnsNull()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);

        // Act
        var result = await repo.ReadMovie(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateMovie_ExistingMovie_UpdatesProperties()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);
        var updates = new Movie(1, "Updated Title", 2025, "Updated Description");

        // Act
        var result = await repo.UpdateMovie(1, updates);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal(2025, result.Year);
        Assert.Equal("Updated Description", result.Description);
    }

    [Fact]
    public async Task UpdateMovie_NonExistingMovie_ReturnsNull()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);
        var updates = new Movie(999, "Updated", 2025, "Description");

        // Act
        var result = await repo.UpdateMovie(999, updates);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateMovie_PersistsInDatabase()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);
        var updates = new Movie(1, "Changed", 2025, "Changed Desc");

        // Act
        await repo.UpdateMovie(1, updates);
        var movie = db.Movies.First(m => m.Id == 1);

        // Assert
        Assert.Equal("Changed", movie.Title);
    }

    [Fact]
    public async Task DeleteMovie_ExistingMovie_RemovesFromDatabase()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);
        var initialCount = db.Movies.Count;

        // Act
        var result = await repo.DeleteMovie(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(initialCount - 1, db.Movies.Count);
        Assert.DoesNotContain(db.Movies, m => m.Id == 1);
    }

    [Fact]
    public async Task DeleteMovie_NonExistingMovie_ReturnsNull()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);

        // Act
        var result = await repo.DeleteMovie(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ReadMovies_WithPageSize1_ReturnsSingleMovie()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);

        // Act
        var result = await repo.ReadMovies(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Values);
    }

    [Fact]
    public async Task CreateMovie_MultipleTimes_IncreasesCount()
    {
        // Arrange
        var db = new MemoryDatabase();
        var repo = new MemoryMovieRepository(db);
        var initialCount = db.Movies.Count;

        // Act
        await repo.CreateMovie(new Movie(0, "Movie 1", 2025, "Desc"));
        await repo.CreateMovie(new Movie(0, "Movie 2", 2025, "Desc"));
        await repo.CreateMovie(new Movie(0, "Movie 3", 2025, "Desc"));

        // Assert
        Assert.Equal(initialCount + 3, db.Movies.Count);
    }
}
