using CodeLens.Domain.Entities;

namespace CodeLens.Application.Interfaces.Chat;

public interface IMessageRepository
{
    Task <List<Message>> GetNByConversationIdAsync(Guid conversationId, int n);
    Task <Message> SaveMessageAsync(Message message);
}