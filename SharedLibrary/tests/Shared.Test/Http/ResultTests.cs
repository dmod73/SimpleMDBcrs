namespace Shared.Test.Http;

using Shared.Http;
using System.Net;

public class ResultTests
{
    [Fact]
    public void Constructor_WithError_SetsIsErrorTrue()
    {
        // Arrange
        var exception = new Exception("Test error");

        // Act
        var result = new Result<string>(exception);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(exception, result.Error);
        Assert.Null(result.Payload);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [Fact]
    public void Constructor_WithErrorAndCustomStatusCode_SetsCorrectStatusCode()
    {
        // Arrange
        var exception = new Exception("Not found");
        var statusCode = (int)HttpStatusCode.NotFound;

        // Act
        var result = new Result<string>(exception, statusCode);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(statusCode, result.StatusCode);
    }

    [Fact]
    public void Constructor_WithPayload_SetsIsErrorFalse()
    {
        // Arrange
        var payload = "Success data";

        // Act
        var result = new Result<string>(payload);

        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Error);
        Assert.Equal(payload, result.Payload);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public void Constructor_WithPayloadAndCustomStatusCode_SetsCorrectStatusCode()
    {
        // Arrange
        var payload = "Created data";
        var statusCode = (int)HttpStatusCode.Created;

        // Act
        var result = new Result<string>(payload, statusCode);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Equal(payload, result.Payload);
    }

    [Fact]
    public void Constructor_WithComplexPayload_StoresCorrectly()
    {
        // Arrange
        var payload = new { Id = 1, Name = "Test" };

        // Act
        var result = new Result<object>(payload);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(payload, result.Payload);
    }

    [Fact]
    public void Constructor_WithNullPayload_AllowsNullPayload()
    {
        // Act
        var result = new Result<string?>((string?)null, (int)HttpStatusCode.NoContent);

        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Payload);
        Assert.Equal((int)HttpStatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public void Result_DifferentGenericTypes_WorkCorrectly()
    {
        // Arrange & Act
        var intResult = new Result<int>(42);
        var stringResult = new Result<string>("test");
        var listResult = new Result<List<int>>(new List<int> { 1, 2, 3 });

        // Assert
        Assert.Equal(42, intResult.Payload);
        Assert.Equal("test", stringResult.Payload);
        Assert.Equal(3, listResult.Payload?.Count);
    }

    [Fact]
    public void Result_WithBadRequestStatus_SetsCorrectly()
    {
        // Arrange
        var error = new ArgumentException("Invalid input");

        // Act
        var result = new Result<string>(error, (int)HttpStatusCode.BadRequest);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }
}
