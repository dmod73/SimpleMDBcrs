namespace Shared.Test.Http;

using Shared.Http;
using System.Collections.Specialized;

public class HttpRouterTests
{
    [Fact]
    public void Constructor_InitializesEmptyRouter()
    {
        // Act
        var router = new HttpRouter();

        // Assert
        Assert.NotNull(router);
    }

    [Fact]
    public void Use_AddsGlobalMiddleware_ReturnsRouter()
    {
        // Arrange
        var router = new HttpRouter();
        HttpMiddleware middleware = async (req, res, props, next) => await next();

        // Act
        var result = router.Use(middleware);

        // Assert
        Assert.Same(router, result);
    }

    [Fact]
    public void MapGet_AddsGetRoute_ReturnsRouter()
    {
        // Arrange
        var router = new HttpRouter();
        HttpMiddleware handler = async (req, res, props, next) => await next();

        // Act
        var result = router.MapGet("/test", handler);

        // Assert
        Assert.Same(router, result);
    }

    [Fact]
    public void MapPost_AddsPostRoute_ReturnsRouter()
    {
        // Arrange
        var router = new HttpRouter();
        HttpMiddleware handler = async (req, res, props, next) => await next();

        // Act
        var result = router.MapPost("/test", handler);

        // Assert
        Assert.Same(router, result);
    }

    [Fact]
    public void MapPut_AddsPutRoute_ReturnsRouter()
    {
        // Arrange
        var router = new HttpRouter();
        HttpMiddleware handler = async (req, res, props, next) => await next();

        // Act
        var result = router.MapPut("/test", handler);

        // Assert
        Assert.Same(router, result);
    }

    [Fact]
    public void MapDelete_AddsDeleteRoute_ReturnsRouter()
    {
        // Arrange
        var router = new HttpRouter();
        HttpMiddleware handler = async (req, res, props, next) => await next();

        // Act
        var result = router.MapDelete("/test", handler);

        // Assert
        Assert.Same(router, result);
    }

    [Fact]
    public void UseSimpleRouteMatching_ReturnsRouter()
    {
        // Arrange
        var router = new HttpRouter();

        // Act
        var result = router.UseSimpleRouteMatching();

        // Assert
        Assert.Same(router, result);
    }

    [Fact]
    public void UseParameterizedRouteMatching_ReturnsRouter()
    {
        // Arrange
        var router = new HttpRouter();

        // Act
        var result = router.UseParameterizedRouteMatching();

        // Assert
        Assert.Same(router, result);
    }

    [Fact]
    public void UseRouter_AddsNestedRouter_ReturnsRouter()
    {
        // Arrange
        var router = new HttpRouter();
        var subRouter = new HttpRouter();

        // Act
        var result = router.UseRouter("/api", subRouter);

        // Assert
        Assert.Same(router, result);
    }

    [Fact]
    public void ParseUrlParams_SimpleMatch_ReturnsParameters()
    {
        // Arrange
        var urlPath = "/users/123";
        var routePath = "/users/:id";

        // Act
        var result = HttpRouter.ParseUrlParams(urlPath, routePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result["id"]);
    }

    [Fact]
    public void ParseUrlParams_MultipleParameters_ReturnsAllParameters()
    {
        // Arrange
        var urlPath = "/users/123/posts/456";
        var routePath = "/users/:userId/posts/:postId";

        // Act
        var result = HttpRouter.ParseUrlParams(urlPath, routePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result["userId"]);
        Assert.Equal("456", result["postId"]);
    }

    [Fact]
    public void ParseUrlParams_NoMatch_ReturnsNull()
    {
        // Arrange
        var urlPath = "/users/123/comments";
        var routePath = "/users/:id/posts";

        // Act
        var result = HttpRouter.ParseUrlParams(urlPath, routePath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseUrlParams_DifferentSegmentCount_ReturnsNull()
    {
        // Arrange
        var urlPath = "/users/123";
        var routePath = "/users/:id/posts";

        // Act
        var result = HttpRouter.ParseUrlParams(urlPath, routePath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseUrlParams_WithTrailingSlash_HandlesCorrectly()
    {
        // Arrange
        var urlPath = "/users/123/";
        var routePath = "/users/:id";

        // Act
        var result = HttpRouter.ParseUrlParams(urlPath, routePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result["id"]);
    }

    [Fact]
    public void ParseUrlParams_UrlEncodedParameter_DecodesCorrectly()
    {
        // Arrange
        var urlPath = "/search/hello%20world";
        var routePath = "/search/:query";

        // Act
        var result = HttpRouter.ParseUrlParams(urlPath, routePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("hello world", result["query"]);
    }

    [Fact]
    public void ParseUrlParams_StaticSegments_MustMatch()
    {
        // Arrange
        var urlPath = "/api/users/123";
        var routePath = "/api/posts/:id";

        // Act
        var result = HttpRouter.ParseUrlParams(urlPath, routePath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseUrlParams_MixedStaticAndDynamic_WorksCorrectly()
    {
        // Arrange
        var urlPath = "/api/v1/users/123/profile";
        var routePath = "/api/v1/users/:id/profile";

        // Act
        var result = HttpRouter.ParseUrlParams(urlPath, routePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result["id"]);
    }

    [Fact]
    public void ParseUrlParams_EmptyPath_ReturnsEmptyCollection()
    {
        // Arrange
        var urlPath = "/";
        var routePath = "/";

        // Act
        var result = HttpRouter.ParseUrlParams(urlPath, routePath);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void RESPONSE_NOT_SENT_HasCorrectValue()
    {
        // Assert
        Assert.Equal(777, HttpRouter.RESPONSE_NOT_SENT);
    }

    [Fact]
    public void Map_WithCustomMethod_AddsRoute()
    {
        // Arrange
        var router = new HttpRouter();
        HttpMiddleware handler = async (req, res, props, next) => await next();

        // Act
        var result = router.Map("PATCH", "/test", handler);

        // Assert
        Assert.Same(router, result);
    }

    [Fact]
    public void Map_MethodIsCaseInsensitive_WorksCorrectly()
    {
        // Arrange
        var router = new HttpRouter();
        HttpMiddleware handler = async (req, res, props, next) => await next();

        // Act
        var result1 = router.Map("get", "/test1", handler);
        var result2 = router.Map("GET", "/test2", handler);
        var result3 = router.Map("Get", "/test3", handler);

        // Assert - all should work without throwing
        Assert.Same(router, result1);
        Assert.Same(router, result2);
        Assert.Same(router, result3);
    }
}
