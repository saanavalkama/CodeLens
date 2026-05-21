using System.Net.Http.Headers;
using System.Text.Json;
using CodeLens.Application.DTOs.User;
using CodeLens.Application.Interfaces.Auth;
using Microsoft.Extensions.Options;
using CodeLens.Infrastructure.Options; 

namespace CodeLens.Infrastructure.Services;

public class GitHubAuthService : IGitHubAuthService
{
    private readonly GitHubOptions _options;
    private readonly HttpClient _httpClient;

    public GitHubAuthService(
        IOptions<GitHubOptions> options,
        HttpClient httpClient
    )
    {
        _options = options.Value;
        _httpClient = httpClient;
    }

    public string GetAuthorizationUrl()
    {
              return $"https://github.com/login/oauth/authorize" +
               $"?client_id={_options.ClientId}" +
               $"&redirect_uri={_options.CallbackUrl}";
    }

    public async Task<GitHubTokenDto>ExchangeCodeForTokenAsync(string code)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, 
            "https://github.com/login/oauth/access_token");

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["code"] = code
        });

        var res = await _httpClient.SendAsync(request);
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

       return ExtractValuesAndBuildDto(result);

    }

    public async Task<GitHubUserDto> GetUserAsync(GitHubTokenDto dto)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", dto.AccessToken);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("CodeLens","1.0"));

        var res = await _httpClient.SendAsync(request);
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        return new GitHubUserDto(
            Id: result.GetProperty("id").GetInt64(),
            Login: result.GetProperty("login").GetString() ?? string.Empty,
            Email: result.TryGetProperty("email", out var email) ? email.GetString() : null,
            AvatarUrl: result.GetProperty("avatar_url").GetString() ?? string.Empty,
            AccessToken: dto.AccessToken,
            RefreshToken: dto.RefreshToken,
            TokenExpiresAt: dto.ExpiresAt
        );

    }

    public async Task<GitHubTokenDto> GitHubRefreshAsync(string rt)
    {
        //once crypting is done it needs to go thru decrypt
        var url = "https://github.com/login/oauth/access_token" + 
            $"?client_id={_options.ClientId}" +
            $"&client_secret={_options.ClientSecret}" +
            $"&grant_type=refresh_token" + 
            $"&refresh_token={rt}";
        var request = new HttpRequestMessage(HttpMethod.Post,url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var res = await _httpClient.SendAsync(request);
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

       return ExtractValuesAndBuildDto(result);
    }

    private GitHubTokenDto ExtractValuesAndBuildDto(JsonElement result)
    {
        var accessToken = result.GetProperty("access_token").GetString()
         ?? throw new Exception("No access token in GitHub refresh response");

        result.TryGetProperty("refresh_token", out var refreshToken);
        result.TryGetProperty("expires_in", out var expiresIn);

        return new GitHubTokenDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken.GetString(),
            ExpiresAt: expiresIn.ValueKind != JsonValueKind.Undefined
                ? DateTime.UtcNow.AddSeconds(expiresIn.GetInt32())
                : null
        );
    }
}