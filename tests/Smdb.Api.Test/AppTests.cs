namespace Smdb.Api.Test;

using Smdb.Api;
using Smdb.Core.Movies;
using Smdb.Core.Db;

public class AppTests
{
    [Fact]
    public void App_InheritsFromHttpServer()
    {
        // Arrange & Act
        var app = new App();

        // Assert
        Assert.IsAssignableFrom<Shared.Http.HttpServer>(app);
    }

    [Fact]
    public void Init_ConfiguresApplication()
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
}
