namespace Shared.Test.Http;

using Shared.Http;
using System.Collections;
using System.Net;
using System.Text.Json;

public class JsonUtilsTests
{
    [Fact]
    public void DefaultOptions_HasCorrectSettings()
    {
        // Act
        var options = JsonUtils.DefaultOptions;

        // Assert
        Assert.True(options.PropertyNameCaseInsensitive);
        Assert.Equal(JsonNamingPolicy.CamelCase, options.PropertyNamingPolicy);
        Assert.NotNull(options.Encoder);
    }

    // Note: GetPaginatedResponse tests removed as they require proper mocking
    // of HttpListenerRequest and HttpListenerResponse which cannot be easily instantiated
    // Consider using a mocking framework like Moq or NSubstitute for these tests

    // Helper methods (kept for reference but not used)
    private HttpListenerRequest CreateMockRequest(string baseUrl, string query)
    {
        // Note: HttpListenerRequest cannot be easily mocked without complex setup
        // This is a simplified version - in real tests, you might use a mocking framework
        // or create a testable wrapper around HttpListenerRequest

        // For now, returning null and acknowledging this would need proper mocking
        // in a production environment (using Moq or NSubstitute)
        return null!;
    }

    private HttpListenerResponse CreateMockResponse()
    {
        return null!;
    }

    [Fact]
    public void DefaultOptions_SerializesObjectsWithCamelCase()
    {
        // Arrange
        var obj = new { FirstName = "John", LastName = "Doe" };

        // Act
        var json = JsonSerializer.Serialize(obj, JsonUtils.DefaultOptions);

        // Assert
        Assert.Contains("firstName", json);
        Assert.Contains("lastName", json);
        Assert.DoesNotContain("FirstName", json);
    }

    [Fact]
    public void DefaultOptions_DeserializesWithCaseInsensitivity()
    {
        // Arrange
        var json = "{\"firstname\":\"John\",\"LASTNAME\":\"Doe\"}";

        // Act
        var obj = JsonSerializer.Deserialize<Person>(json, JsonUtils.DefaultOptions);

        // Assert
        Assert.NotNull(obj);
        Assert.Equal("John", obj.FirstName);
        Assert.Equal("Doe", obj.LastName);
    }

    private class Person
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
