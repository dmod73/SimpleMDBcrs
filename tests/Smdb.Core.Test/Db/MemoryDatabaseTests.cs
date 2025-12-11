namespace Smdb.Core.Test.Db;

using Smdb.Core.Db;
using Smdb.Core.Movies;

public class MemoryDatabaseTests
{
    [Fact]
    public void Constructor_InitializesWithMovies()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        Assert.NotNull(db.Movies);
        Assert.NotEmpty(db.Movies);
    }

    [Fact]
    public void Constructor_SeedsExactly50Movies()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        Assert.Equal(50, db.Movies.Count);
    }

    [Fact]
    public void Movies_ContainsTheGodfather()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        var godfather = db.Movies.FirstOrDefault(m => m.Title == "The Godfather");
        Assert.NotNull(godfather);
        Assert.Equal(1972, godfather.Year);
    }

    [Fact]
    public void Movies_AllHaveUniqueIds()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        var ids = db.Movies.Select(m => m.Id).ToList();
        var uniqueIds = ids.Distinct().ToList();
        Assert.Equal(ids.Count, uniqueIds.Count);
    }

    [Fact]
    public void Movies_AllHaveTitles()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        Assert.All(db.Movies, movie => Assert.False(string.IsNullOrWhiteSpace(movie.Title)));
    }

    [Fact]
    public void Movies_AllHaveDescriptions()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        Assert.All(db.Movies, movie => Assert.False(string.IsNullOrWhiteSpace(movie.Description)));
    }

    [Fact]
    public void Movies_AllHaveValidYears()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        Assert.All(db.Movies, movie =>
        {
            Assert.True(movie.Year >= 1888);
            Assert.True(movie.Year <= DateTime.UtcNow.Year);
        });
    }

    [Fact]
    public void NextMovieId_ReturnsIncrementingIds()
    {
        // Arrange
        var db = new MemoryDatabase();

        // Act
        var id1 = db.NextMovieId();
        var id2 = db.NextMovieId();
        var id3 = db.NextMovieId();

        // Assert
        Assert.Equal(51, id1);
        Assert.Equal(52, id2);
        Assert.Equal(53, id3);
    }

    [Fact]
    public void Movies_IsMutableList()
    {
        // Arrange
        var db = new MemoryDatabase();
        var initialCount = db.Movies.Count;

        // Act
        db.Movies.Add(new Movie(999, "Test Movie", 2020, "Test"));

        // Assert
        Assert.Equal(initialCount + 1, db.Movies.Count);
    }

    [Fact]
    public void Movies_CanBeRemoved()
    {
        // Arrange
        var db = new MemoryDatabase();
        var movieToRemove = db.Movies.First();

        // Act
        db.Movies.Remove(movieToRemove);

        // Assert
        Assert.DoesNotContain(movieToRemove, db.Movies);
    }

    [Fact]
    public void Movies_ContainsClassicFilms()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        var classicTitles = new[] { "The Godfather", "The Shawshank Redemption", "Pulp Fiction", "The Matrix" };
        foreach (var title in classicTitles)
        {
            Assert.Contains(db.Movies, m => m.Title == title);
        }
    }

    [Fact]
    public void Movies_SpansMultipleDecades()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        var years = db.Movies.Select(m => m.Year).ToList();
        Assert.Contains(years, y => y < 2000); // Old movies
        Assert.Contains(years, y => y >= 2000 && y < 2010); // 2000s
        Assert.Contains(years, y => y >= 2010); // Recent movies
    }

    [Fact]
    public void NextMovieId_AfterMultipleInstances_IsIndependent()
    {
        // Arrange
        var db1 = new MemoryDatabase();
        var db2 = new MemoryDatabase();

        // Act
        var id1 = db1.NextMovieId();
        var id2 = db2.NextMovieId();

        // Assert
        Assert.Equal(id1, id2); // Each instance starts independently
    }

    [Fact]
    public void Movies_IdsStartFrom1()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        var firstMovie = db.Movies.First();
        Assert.Equal(1, firstMovie.Id);
    }

    [Fact]
    public void Movies_IdsAreSequential()
    {
        // Act
        var db = new MemoryDatabase();

        // Assert
        var ids = db.Movies.Select(m => m.Id).OrderBy(id => id).ToList();
        for (int i = 0; i < ids.Count; i++)
        {
            Assert.Equal(i + 1, ids[i]);
        }
    }
}
