namespace CodeLens.Application.Interfaces.Chat;
public interface IConversationService
{
    Task <ConversationResponseDto> CreateAsync(Guid userId, Guid repoId);
    Task<List<ConversationResponseDto>>GetConversationsByRepoIdAsync(Guid userId, Guid repoId);
}