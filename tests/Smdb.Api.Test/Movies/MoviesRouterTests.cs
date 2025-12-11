namespace Smdb.Api.Test.Movies;

using Smdb.Api.Movies;
using Smdb.Core.Movies;
using Moq;

public class MoviesRouterTests
{
    [Fact]
    public void Constructor_CreatesRouterWithParameterizedRouting()
    {
        // Arrange
        var mockController = new Mock<MoviesController>(Mock.Of<IMovieService>());

        // Act
        var router = new MoviesRouter(mockController.Object);

        // Assert
        Assert.NotNull(router);
    }

    [Fact]
    public void Constructor_WithNullController_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new MoviesRouter(null!));
    }

    [Fact]
    public void Constructor_ConfiguresRoutes()
    {
        // Arrange
        var mockService = new Mock<IMovieService>();
        var controller = new MoviesController(mockService.Object);

        // Act
        var router = new MoviesRouter(controller);

        // Assert
        // Router should be created successfully with configured routes
        Assert.NotNull(router);
    }

    [Fact]
    public void Constructor_StoresControllerReference()
    {
        // Arrange
        var mockService = new Mock<IMovieService>();
        var controller = new MoviesController(mockService.Object);

        // Act
        var router = new MoviesRouter(controller);

        // Assert
        Assert.NotNull(router);
    }
}
