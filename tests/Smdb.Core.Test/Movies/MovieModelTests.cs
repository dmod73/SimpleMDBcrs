namespace Smdb.Core.Test.Movies;

using Smdb.Core.Movies;

public class MovieModelTests
{
    [Fact]
    public void Constructor_WithValidData_SetsPropertiesCorrectly()
    {
        // Arrange
        var id = 1;
        var title = "The Godfather";
        var year = 1972;
        var description = "A mafia patriarch hands the family empire to his reluctant son.";

        // Act
        var movie = new Movie(id, title, year, description);

        // Assert
        Assert.Equal(id, movie.Id);
        Assert.Equal(title, movie.Title);
        Assert.Equal(year, movie.Year);
        Assert.Equal(description, movie.Description);
    }

    [Fact]
    public void Constructor_WithEmptyTitle_CreatesMovie()
    {
        // Act
        var movie = new Movie(1, "", 2000, "Description");

        // Assert
        Assert.Equal("", movie.Title);
    }

    [Fact]
    public void Constructor_WithEmptyDescription_CreatesMovie()
    {
        // Act
        var movie = new Movie(1, "Title", 2000, "");

        // Assert
        Assert.Equal("", movie.Description);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var movie = new Movie(1, "Test Movie", 2020, "Test Description");

        // Act
        var result = movie.ToString();

        // Assert
        Assert.Contains("Movie[", result);
        Assert.Contains("Id=1", result);
        Assert.Contains("Title=Test Movie", result);
        Assert.Contains("Year=2020", result);
        Assert.Contains("Description=Test Description", result);
    }

    [Fact]
    public void Properties_AreSettable()
    {
        // Arrange
        var movie = new Movie(1, "Original", 2000, "Original Desc");

        // Act
        movie.Id = 2;
        movie.Title = "Updated";
        movie.Year = 2010;
        movie.Description = "Updated Desc";

        // Assert
        Assert.Equal(2, movie.Id);
        Assert.Equal("Updated", movie.Title);
        Assert.Equal(2010, movie.Year);
        Assert.Equal("Updated Desc", movie.Description);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var title = "Movie: The \"Special\" Edition";
        var description = "Description with 'quotes' and symbols #@!";

        // Act
        var movie = new Movie(1, title, 2020, description);

        // Assert
        Assert.Equal(title, movie.Title);
        Assert.Equal(description, movie.Description);
    }

    [Fact]
    public void Constructor_WithLongStrings_HandlesCorrectly()
    {
        // Arrange
        var longTitle = new string('a', 500);
        var longDesc = new string('b', 1000);

        // Act
        var movie = new Movie(1, longTitle, 2020, longDesc);

        // Assert
        Assert.Equal(500, movie.Title.Length);
        Assert.Equal(1000, movie.Description.Length);
    }

    [Fact]
    public void Constructor_WithZeroId_CreatesMovie()
    {
        // Act
        var movie = new Movie(0, "Title", 2020, "Description");

        // Assert
        Assert.Equal(0, movie.Id);
    }

    [Fact]
    public void Constructor_WithNegativeId_CreatesMovie()
    {
        // Act
        var movie = new Movie(-1, "Title", 2020, "Description");

        // Assert
        Assert.Equal(-1, movie.Id);
    }

    [Fact]
    public void Constructor_WithOldYear_CreatesMovie()
    {
        // Act
        var movie = new Movie(1, "Ancient Film", 1888, "First motion picture");

        // Assert
        Assert.Equal(1888, movie.Year);
    }

    [Fact]
    public void Constructor_WithFutureYear_CreatesMovie()
    {
        // Act
        var movie = new Movie(1, "Future Film", 2050, "Science fiction");

        // Assert
        Assert.Equal(2050, movie.Year);
    }
}
