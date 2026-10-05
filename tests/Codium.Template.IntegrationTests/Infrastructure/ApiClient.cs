using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Codium.Template.IntegrationTests.Infrastructure;

/// <summary>Small helper around <see cref="HttpClient"/> for the template's JSON API.</summary>
public class ApiClient(HttpClient http)
{
    public const string AdminEmail = "admin@codium.com";
    public const string AdminPassword = "Pp123456*";
    public const string DefaultPassword = "Test1234*";

    public static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    public HttpClient Http { get; } = http;

    public sealed record Response(HttpStatusCode Status, JsonElement Body)
    {
        public JsonElement Data => Body.GetProperty("data");
        public string? ErrorCode => Body.ValueKind == JsonValueKind.Object && Body.TryGetProperty("ErrorCode", out var e) ? e.GetString() : null;
    }

    public async Task<Response> SendAsync(HttpMethod method, string url, object? body = null, string? token = null)
    {
        using var request = new HttpRequestMessage(method, url);
        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (body != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }

        using var response = await Http.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        var json = string.IsNullOrWhiteSpace(text) ? default : JsonDocument.Parse(text).RootElement.Clone();
        return new Response(response.StatusCode, json);
    }

    public Task<Response> GetAsync(string url, string? token = null) => SendAsync(HttpMethod.Get, url, null, token);
    public Task<Response> PostAsync(string url, object? body, string? token = null) => SendAsync(HttpMethod.Post, url, body, token);
    public Task<Response> PutAsync(string url, object? body, string? token = null) => SendAsync(HttpMethod.Put, url, body, token);
    public Task<Response> PatchAsync(string url, object? body, string? token = null) => SendAsync(HttpMethod.Patch, url, body, token);
    public Task<Response> DeleteAsync(string url, string? token = null) => SendAsync(HttpMethod.Delete, url, null, token);

    public Task<Response> LoginAsync(string email, string password) =>
        PostAsync("/api/v1/auth/login", new { email, password });

    public async Task<string> LoginTokenAsync(string email, string password)
    {
        var response = await LoginAsync(email, password);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        return response.Data.GetProperty("accessToken").GetString()!;
    }

    public Task<string> AdminTokenAsync() => LoginTokenAsync(AdminEmail, AdminPassword);

    public async Task<Guid> CreateUserAsync(string adminToken, string email, string password = DefaultPassword,
        bool emailConfirmed = true, bool isActive = true)
    {
        var response = await PostAsync("/api/v1/users", new
        {
            email,
            password,
            confirmPassword = password,
            emailConfirmed,
            isActive,
            firstName = "Test",
            lastName = "User"
        }, adminToken);
        Assert.Equal(HttpStatusCode.NoContent, response.Status);
        return await FindUserIdAsync(adminToken, email);
    }

    public async Task<Guid> FindUserIdAsync(string adminToken, string email)
    {
        var list = await GetAsync($"/api/v1/users/paged?search={Uri.EscapeDataString(email)}", adminToken);
        Assert.Equal(HttpStatusCode.OK, list.Status);
        return list.Data.GetProperty("data")[0].GetProperty("id").GetGuid();
    }

    public async Task<Guid> CreateRoleAsync(string adminToken, string name)
    {
        var response = await PostAsync("/api/v1/roles", new { name, description = "test role" }, adminToken);
        Assert.Equal(HttpStatusCode.NoContent, response.Status);
        var list = await GetAsync($"/api/v1/roles/paged?search={Uri.EscapeDataString(name)}", adminToken);
        return list.Data.GetProperty("data")[0].GetProperty("id").GetGuid();
    }

    public static JsonElement ReadJwtPayload(string jwt)
    {
        var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        return JsonDocument.Parse(Convert.FromBase64String(payload)).RootElement.Clone();
    }
}
