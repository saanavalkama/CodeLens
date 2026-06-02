using CodeLens.Application.Interfaces.Chat;
using CodeLens.Domain.Entities;
using CodeLens.Domain.Exceptions;
using CodeLens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeLens.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;

    public ConversationRepository(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<Conversation>CreateAsync(Conversation conversation)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();
        return conversation;
    }

    public async Task<List<Conversation>>GetByRepoIdAsync(Guid repoId)
    {
        return await _context.Conversations
            .Where(c => c.RepositoryId == repoId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Conversation?>GetByIdAsync(Guid id)
    {
        return await _context.Conversations.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Conversations.FirstOrDefaultAsync(c => c.Id == id);
        if(entity == null)throw new NotFoundException("Entity");
        _context.Conversations.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task <Conversation> UpdateAsync(Conversation conversation)
    {
        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync();
        return conversation;
    }
}