using CodeLens.Application.Interfaces.Chat;
using CodeLens.Domain.Entities;
using CodeLens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeLens.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task <List<Message>> GetNByConversationIdAsync(Guid conversationId, int n)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(n)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
        return messages;
    }

    public async Task <Message> SaveMessageAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }
}