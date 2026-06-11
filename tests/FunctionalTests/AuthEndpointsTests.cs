using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FunctionalTests;

public sealed class AuthEndpointsTests : IClassFixture<HabitatWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(HabitatWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ReturnsJwtToken_WhenCredentialsAreValid()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "test@local",
            password = "Password123!"
        });

        var body = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 200 OK for valid login. Status: {(int)response.StatusCode} {response.StatusCode}. Body: {body}");

        using var json = JsonDocument.Parse(body);

        Assert.True(
            json.RootElement.TryGetProperty("accessToken", out var token),
            $"Login response did not contain 'accessToken' at root level. Body: {body}");

        Assert.False(
            string.IsNullOrWhiteSpace(token.GetString()),
            $"Login response contained an empty accessToken. Body: {body}");

        Assert.True(
            json.RootElement.TryGetProperty("tokenType", out var tokenType),
            $"Login response did not contain 'tokenType'. Body: {body}");

        Assert.Equal("Bearer", tokenType.GetString());

        Assert.True(
            json.RootElement.TryGetProperty("expiresIn", out var expiresIn),
            $"Login response did not contain 'expiresIn'. Body: {body}");

        Assert.True(
            expiresIn.GetInt32() > 0,
            $"Login response contained an invalid expiresIn. Body: {body}");

        Assert.True(
            json.RootElement.TryGetProperty("user", out var user),
            $"Login response did not contain 'user'. Body: {body}");

        Assert.Equal("test@local", user.GetProperty("email").GetString());
        Assert.Equal("USER", user.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Login_ReturnsUnauthorizedProblem_WhenPasswordIsInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "test@local",
            password = "wrong-password"
        });

        var body = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected 401 Unauthorized for invalid password. Status: {(int)response.StatusCode} {response.StatusCode}. Body: {body}");

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(body);

        Assert.True(
            json.RootElement.TryGetProperty("status", out var status),
            $"ProblemDetails response did not contain 'status'. Body: {body}");

        Assert.Equal(401, status.GetInt32());

        Assert.True(
            json.RootElement.TryGetProperty("title", out var title),
            $"ProblemDetails response did not contain 'title'. Body: {body}");

        Assert.Equal("Unauthorized", title.GetString());
    }

    [Fact]
    public async Task Register_ReturnsCreated_WhenEmailIsAvailable()
    {
        var uniqueEmail = $"novo-{Guid.NewGuid():N}@email.com";

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Novo Usuário",
            email = uniqueEmail,
            password = "Senha@123"
        });

        var body = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created for register. Status: {(int)response.StatusCode} {response.StatusCode}. Body: {body}");

        using var json = JsonDocument.Parse(body);

        Assert.True(
            json.RootElement.TryGetProperty("id", out var id),
            $"Register response did not contain 'id'. Body: {body}");

        Assert.True(id.GetInt32() > 0);

        Assert.Equal("Novo Usuário", json.RootElement.GetProperty("name").GetString());
        Assert.Equal(uniqueEmail, json.RootElement.GetProperty("email").GetString());
        Assert.Equal("USER", json.RootElement.GetProperty("role").GetString());
    }
}