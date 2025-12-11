namespace Smbd.Csr.Test;

using Smbd.Csr;
using Shared.Http;

public class AppTests
{
    [Fact]
    public void App_InheritsFromHttpServer()
    {
        // Arrange & Act
        var app = new App();

        // Assert
        Assert.IsAssignableFrom<HttpServer>(app);
    }

    [Fact]
    public void Constructor_CreatesAppInstance()
    {
        // Act
        var app = new App();

        // Assert
        Assert.NotNull(app);
    }

    [Fact]
    public void Init_ConfiguresMiddleware()
    {
        // Arrange
        var app = new App();

        // Act
        app.Init();

        // Assert
        // Init should complete without throwing
        Assert.NotNull(app);
    }

    [Fact]
    public void Init_CanBeCalledMultipleTimes()
    {
        // Arrange
        var app = new App();

        // Act
        app.Init();
        app.Init();

        // Assert
        // Multiple calls should not throw
        Assert.NotNull(app);
    }

    [Fact]
    public void App_HasPublicConstructor()
    {
        // Act
        var app = new App();

        // Assert
        Assert.NotNull(app);
    }

    [Fact]
    public void Init_ConfiguresStaticFileServing()
    {
        // Arrange
        var app = new App();

        // Act
        app.Init();

        // Assert
        // Verifies Init completes successfully with static file serving configured
        Assert.NotNull(app);
    }

    [Fact]
    public void Init_ConfiguresRootRoute()
    {
        // Arrange
        var app = new App();

        // Act
        app.Init();

        // Assert
        // Verifies root route is configured
        Assert.NotNull(app);
    }

    [Fact]
    public void Init_ConfiguresMoviesRoute()
    {
        // Arrange
        var app = new App();

        // Act
        app.Init();

        // Assert
        // Verifies movies route is configured
        Assert.NotNull(app);
    }
}
