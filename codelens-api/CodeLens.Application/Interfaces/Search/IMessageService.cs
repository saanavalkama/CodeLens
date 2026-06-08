using System.Runtime.CompilerServices;
using CodeLens.Application.DTOs.Chat;
using CodeLens.Application.DTOs.Search;

namespace CodeLens.Application.Interfaces.Search;

public interface IMessageService
{
    Task<SearchResponseDto>SearchAsync(Guid userId, Guid repoId, Guid conversationId, string message, CancellationToken ct);
    Task<List<MessageDto>>GetNMessagesAync(Guid userId, Guid repoId, Guid conversationId);
}