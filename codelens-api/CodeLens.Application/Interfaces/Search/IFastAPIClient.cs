using CodeLens.Application.DTOs.Search;

namespace CodeLens.Application.Interfaces.Search;

public interface IFastApiClient
{
    Task<SearchResponseDto> SearchAsync(SearchRequestDto request,CancellationToken ct);

}