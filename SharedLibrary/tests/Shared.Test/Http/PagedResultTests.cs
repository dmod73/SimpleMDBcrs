namespace Shared.Test.Http;

using Shared.Http;

public class PagedResultTests
{
    [Fact]
    public void Constructor_WithValidData_SetsPropertiesCorrectly()
    {
        // Arrange
        var totalCount = 100;
        var values = new List<string> { "Item1", "Item2", "Item3" };

        // Act
        var result = new PagedResult<string>(totalCount, values);

        // Assert
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(values, result.Values);
        Assert.Equal(3, result.Values.Count);
    }

    [Fact]
    public void Constructor_WithEmptyList_CreatesValidObject()
    {
        // Arrange
        var totalCount = 0;
        var values = new List<int>();

        // Act
        var result = new PagedResult<int>(totalCount, values);

        // Assert
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Values);
    }

    [Fact]
    public void Constructor_WithComplexObjects_StoresCorrectly()
    {
        // Arrange
        var items = new List<Movie>
        {
            new Movie { Id = 1, Title = "Movie 1" },
            new Movie { Id = 2, Title = "Movie 2" }
        };

        // Act
        var result = new PagedResult<Movie>(2, items);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Values.Count);
        Assert.Equal("Movie 1", result.Values[0].Title);
    }

    [Fact]
    public void Constructor_TotalCountGreaterThanValues_IsValid()
    {
        // Arrange
        var totalCount = 100;
        var values = new List<string> { "Item1", "Item2" }; // Only 2 items

        // Act
        var result = new PagedResult<string>(totalCount, values);

        // Assert
        Assert.Equal(100, result.TotalCount);
        Assert.Equal(2, result.Values.Count);
    }

    [Fact]
    public void Constructor_WithDifferentTypes_WorksCorrectly()
    {
        // Arrange & Act
        var stringResult = new PagedResult<string>(10, new List<string> { "a", "b" });
        var intResult = new PagedResult<int>(5, new List<int> { 1, 2, 3 });
        var doubleResult = new PagedResult<double>(3, new List<double> { 1.1, 2.2 });

        // Assert
        Assert.Equal(10, stringResult.TotalCount);
        Assert.Equal(5, intResult.TotalCount);
        Assert.Equal(3, doubleResult.TotalCount);
    }

    [Fact]
    public void Values_IsMutable_CanModifyList()
    {
        // Arrange
        var values = new List<string> { "Item1" };
        var result = new PagedResult<string>(1, values);

        // Act
        result.Values.Add("Item2");

        // Assert
        Assert.Equal(2, result.Values.Count);
    }

    private class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
