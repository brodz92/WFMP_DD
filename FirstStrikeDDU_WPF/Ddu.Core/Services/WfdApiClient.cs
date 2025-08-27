using Polly;
using Polly.Retry;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ddu.Core.Services;

public record WfdConnection(string BaseUrl, string ClientId, string ClientSecret, string AppKey, string Username, string Password, string Tenant);
public record WfdAuthToken(string access_token, int expires_in);

public class WfdApiClient(HttpClient http, JsonSerializerOptions? jsonOptions = null)
{
    private readonly HttpClient _http = http;
    private readonly JsonSerializerOptions _json = jsonOptions ?? new(JsonSerializerDefaults.Web);
    private string? _bearer;
    private readonly AsyncRetryPolicy _retry = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(5, i => TimeSpan.FromMilliseconds(200 * i));

    public async Task AuthenticateAsync(WfdConnection conn, CancellationToken ct = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>{
            ["grant_type"] = "password",
            ["username"] = conn.Username,
            ["password"] = conn.Password,
            ["client_id"] = conn.ClientId,
            ["client_secret"] = conn.ClientSecret,
            ["scope"] = "public_api"
        });
        var url = $"{conn.BaseUrl.TrimEnd('/')}/auth/oauth/token";
        var resp = await _retry.ExecuteAsync(ct => _http.PostAsync(url, content, ct), ct);
        resp.EnsureSuccessStatusCode();
        var tok = await JsonSerializer.DeserializeAsync<WfdAuthToken>(await resp.Content.ReadAsStreamAsync(ct), _json, ct);
        _bearer = tok?.access_token ?? throw new InvalidOperationException("No access token");
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearer);
        _http.DefaultRequestHeaders.Remove("appkey");
        _http.DefaultRequestHeaders.Add("appkey", conn.AppKey);
    }

    public async Task<HttpResponseMessage> PostJsonAsync(string path, object payload, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_bearer)) throw new InvalidOperationException("Not authenticated");
        var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : $"{_http.BaseAddress}{path}";
        var json = JsonSerializer.Serialize(payload, _json);
        var resp = await _retry.ExecuteAsync(ct => _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), ct), ct);
        return resp;
    }
}