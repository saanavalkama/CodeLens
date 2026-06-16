using System.Net.Http.Json;
using System.Text.Json;
using CodeLens.Application.DTOs.Search;
using CodeLens.Application.Interfaces.Search;

namespace CodeLens.Infrastructure.Services;

public class FastAPIClient : IFastApiClient
{
    private readonly HttpClient _httpClient;

    public FastAPIClient(
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
    }

    private static readonly JsonSerializerOptions _requestOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions _responseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task <SearchResponseDto>SearchAsync(SearchRequestDto request, CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync("/search", request, _requestOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SearchResponseDto>(_responseOptions, ct);
        return result!;
    }
}