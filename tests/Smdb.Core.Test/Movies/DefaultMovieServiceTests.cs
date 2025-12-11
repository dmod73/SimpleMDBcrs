namespace Smdb.Core.Test.Movies;

using Smdb.Core.Movies;
using Smdb.Core.Db;
using Shared.Http;
using System.Net;
using Moq;

public class DefaultMovieServiceTests
{
    [Fact]
    public async Task ReadMovies_ValidPagination_ReturnsSuccess()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var pagedResult = new PagedResult<Movie>(2, new List<Movie>
        {
            new Movie(1, "Movie 1", 2020, "Description 1"),
            new Movie(2, "Movie 2", 2021, "Description 2")
        });
        mockRepo.Setup(r => r.ReadMovies(1, 10)).ReturnsAsync(pagedResult);
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.ReadMovies(1, 10);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Payload);
        Assert.Equal(2, result.Payload.TotalCount);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task ReadMovies_PageLessThan1_ReturnsBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.ReadMovies(0, 10);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Page must be >= 1", result.Error!.Message);
    }

    [Fact]
    public async Task ReadMovies_SizeLessThan1_ReturnsBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.ReadMovies(1, 0);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Page size must be >= 1", result.Error!.Message);
    }

    [Fact]
    public async Task ReadMovies_RepositoryReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        mockRepo.Setup(r => r.ReadMovies(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((PagedResult<Movie>?)null);
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.ReadMovies(1, 10);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task CreateMovie_ValidMovie_ReturnsCreated()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var movie = new Movie(0, "New Movie", 2020, "Description");
        var createdMovie = new Movie(1, "New Movie", 2020, "Description");
        mockRepo.Setup(r => r.CreateMovie(It.IsAny<Movie>())).ReturnsAsync(createdMovie);
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.CreateMovie(movie);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Payload);
        Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
    }

    [Fact]
    public async Task CreateMovie_NullMovie_ReturnsBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.CreateMovie(null!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Movie payload is required", result.Error!.Message);
    }

    [Fact]
    public async Task CreateMovie_EmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var service = new DefaultMovieService(mockRepo.Object);
        var movie = new Movie(0, "", 2020, "Description");

        // Act
        var result = await service.CreateMovie(movie);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Title is required", result.Error!.Message);
    }

    [Fact]
    public async Task CreateMovie_TitleTooLong_ReturnsBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var service = new DefaultMovieService(mockRepo.Object);
        var movie = new Movie(0, new string('a', 257), 2020, "Description");

        // Act
        var result = await service.CreateMovie(movie);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("cannot be longer than 256 characters", result.Error!.Message);
    }

    [Fact]
    public async Task CreateMovie_YearTooOld_ReturnsBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var service = new DefaultMovieService(mockRepo.Object);
        var movie = new Movie(0, "Movie", 1887, "Description");

        // Act
        var result = await service.CreateMovie(movie);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Year must be between 1888", result.Error!.Message);
    }

    [Fact]
    public async Task CreateMovie_YearInFuture_ReturnsBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var service = new DefaultMovieService(mockRepo.Object);
        var futureYear = DateTime.UtcNow.Year + 1;
        var movie = new Movie(0, "Movie", futureYear, "Description");

        // Act
        var result = await service.CreateMovie(movie);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task ReadMovie_ExistingId_ReturnsMovie()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var movie = new Movie(1, "Movie", 2020, "Description");
        mockRepo.Setup(r => r.ReadMovie(1)).ReturnsAsync(movie);
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.ReadMovie(1);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Payload);
        Assert.Equal(1, result.Payload.Id);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task ReadMovie_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        mockRepo.Setup(r => r.ReadMovie(It.IsAny<int>())).ReturnsAsync((Movie?)null);
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.ReadMovie(999);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task UpdateMovie_ValidData_ReturnsSuccess()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var movie = new Movie(1, "Updated", 2020, "Description");
        mockRepo.Setup(r => r.UpdateMovie(1, It.IsAny<Movie>())).ReturnsAsync(movie);
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.UpdateMovie(1, movie);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task UpdateMovie_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var service = new DefaultMovieService(mockRepo.Object);
        var movie = new Movie(1, "", 2020, "Description");

        // Act
        var result = await service.UpdateMovie(1, movie);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task UpdateMovie_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        mockRepo.Setup(r => r.UpdateMovie(It.IsAny<int>(), It.IsAny<Movie>())).ReturnsAsync((Movie?)null);
        var service = new DefaultMovieService(mockRepo.Object);
        var movie = new Movie(999, "Movie", 2020, "Description");

        // Act
        var result = await service.UpdateMovie(999, movie);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteMovie_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        var movie = new Movie(1, "Movie", 2020, "Description");
        mockRepo.Setup(r => r.DeleteMovie(1)).ReturnsAsync(movie);
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.DeleteMovie(1);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task DeleteMovie_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var mockRepo = new Mock<IMovieRepository>();
        mockRepo.Setup(r => r.DeleteMovie(It.IsAny<int>())).ReturnsAsync((Movie?)null);
        var service = new DefaultMovieService(mockRepo.Object);

        // Act
        var result = await service.DeleteMovie(999);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
    }
}
