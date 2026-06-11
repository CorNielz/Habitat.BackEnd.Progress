using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FunctionalTests;

public sealed class ProtectedEndpointsTests : IClassFixture<HabitatWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public ProtectedEndpointsTests(HabitatWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UsersMe_ReturnsUnauthorized_WhenTokenIsMissing()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task HabitsCrud_WorksForAuthenticatedUser()
    {
        var token = await AuthenticateAsync("test@local", "Password123!");

        using var createRequest = CreateAuthorizedJsonRequest(
            HttpMethod.Post,
            "/api/v1/habits",
            token,
            new
            {
                title = $"Ler {Guid.NewGuid():N}",
                description = "Leitura diária",
                frequencyType = "DAILY",
                frequencyValue = "1",
                startDate = "2026-05-01"
            });

        var create = await _client.SendAsync(createRequest);
        var createBody = await create.Content.ReadAsStringAsync();

        Assert.True(
            create.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created when creating habit. Status: {(int)create.StatusCode} {create.StatusCode}. Body: {createBody}");

        using var createdJson = JsonDocument.Parse(createBody);

        Assert.True(
            createdJson.RootElement.TryGetProperty("id", out var createdIdProperty),
            $"Create habit response did not contain 'id'. Body: {createBody}");

        var createdHabitId = createdIdProperty.GetInt32();

        using var listRequest = CreateAuthorizedJsonRequest(
            HttpMethod.Get,
            "/api/v1/habits?page=1&pageSize=20",
            token);

        var list = await _client.SendAsync(listRequest);
        var listBody = await list.Content.ReadAsStringAsync();

        Assert.True(
            list.StatusCode == HttpStatusCode.OK,
            $"Expected 200 OK when listing habits. Status: {(int)list.StatusCode} {list.StatusCode}. Body: {listBody}");

        using var listJson = JsonDocument.Parse(listBody);

        Assert.True(
            listJson.RootElement.TryGetProperty("items", out var items),
            $"List habits response did not contain 'items'. Body: {listBody}");

        var createdHabitWasReturned = items
            .EnumerateArray()
            .Any(item =>
                item.TryGetProperty("id", out var idProperty)
                && idProperty.GetInt32() == createdHabitId);

        Assert.True(
            createdHabitWasReturned,
            $"Created habit with id {createdHabitId} was not returned in the list response. Body: {listBody}");
    }

    [Fact]
    public async Task AdminEndpoint_ReturnsForbidden_ForCommonUser()
    {
        var token = await AuthenticateAsync("test@local", "Password123!");

        using var request = CreateAuthorizedJsonRequest(
            HttpMethod.Get,
            "/api/v1/admin/users",
            token);

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden,
            $"Expected 403 Forbidden for common user accessing admin endpoint. Status: {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
    }

    private async Task<string> AuthenticateAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password
        });

        var body = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.IsSuccessStatusCode,
            $"Login failed. Status: {(int)response.StatusCode} {response.StatusCode}. Body: {body}");

        var loginResponse = JsonSerializer.Deserialize<LoginResponseForTest>(
            body,
            JsonOptions);

        Assert.NotNull(loginResponse);

        Assert.False(
            string.IsNullOrWhiteSpace(loginResponse.AccessToken),
            $"Login response did not contain a valid accessToken. Body: {body}");

        return loginResponse.AccessToken;
    }

    private static HttpRequestMessage CreateAuthorizedJsonRequest(
        HttpMethod method,
        string requestUri,
        string token,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, requestUri);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private sealed class LoginResponseForTest
    {
        public string AccessToken { get; init; } = string.Empty;
    }
}