using System.Text.Json;

namespace ContractTests;

public sealed class OpenApiContractTests
{
    [Fact]
    public async Task Contract_ContainsAllVersionOnePaths()
    {
        await using var stream = File.OpenRead("habitat-progress-openapi-v1.json");
        using var document = await JsonDocument.ParseAsync(stream);
        var paths = document.RootElement.GetProperty("paths");

        string[] expectedPaths =
        [
            "/auth/register",
            "/auth/login",
            "/users/me",
            "/users/me/password",
            "/settings",
            "/habits",
            "/habits/{id}",
            "/habits/{habitId}/records",
            "/notes",
            "/notes/{id}",
            "/dashboard",
            "/dashboard/history",
            "/admin/users",
            "/admin/users/{id}",
            "/admin/users/{id}/role"
        ];

        foreach (var path in expectedPaths)
        {
            Assert.True(paths.TryGetProperty(path, out _), $"OpenAPI contract is missing {path}.");
        }
    }

    [Fact]
    public async Task Contract_UsesJwtBearerSecurityScheme()
    {
        await using var stream = File.OpenRead("habitat-progress-openapi-v1.json");
        using var document = await JsonDocument.ParseAsync(stream);
        var bearer = document.RootElement
            .GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("bearerAuth");

        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());
        Assert.Equal("JWT", bearer.GetProperty("bearerFormat").GetString());
    }

    [Fact]
    public async Task Contract_UsesIntegerIdentifiers()
    {
        await using var stream = File.OpenRead("habitat-progress-openapi-v1.json");
        using var document = await JsonDocument.ParseAsync(stream);
        var id = document.RootElement.GetProperty("components").GetProperty("parameters").GetProperty("Id").GetProperty("schema");

        Assert.Equal("integer", id.GetProperty("type").GetString());
        Assert.Equal("int32", id.GetProperty("format").GetString());
    }
}
