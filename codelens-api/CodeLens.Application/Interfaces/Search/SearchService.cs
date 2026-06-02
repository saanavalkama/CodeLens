using System.Runtime.CompilerServices;
using CodeLens.Application.DTOs.Search;

namespace CodeLens.Application.Interfaces.Search;

public interface ISearchService
{
    Task<SearchResponseDto>SearchAsync(Guid userId, Guid repoId, Guid conversationId, string message, CancellationToken ct);
}