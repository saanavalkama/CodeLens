using System.Net.Http.Json;
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

    public async Task <SearchResponseDto>SearchAsync(SearchRequestDto request, CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync("/search", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SearchResponseDto>(ct);
        return result!;
    }
}