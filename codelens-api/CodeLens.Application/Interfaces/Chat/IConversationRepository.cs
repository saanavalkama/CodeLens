using CodeLens.Domain.Entities;

namespace CodeLens.Application.Interfaces.Chat;

public interface IConversationRepository
{
    Task<Conversation>CreateAsync(Conversation conversation);
    Task<List<Conversation>>GetByRepoIdAsync(Guid repoId);

    Task<Conversation?>GetByIdAsync(Guid id);

    Task DeleteAsync(Guid id);

    Task <Conversation> UpdateAsync(Conversation conversation);
}