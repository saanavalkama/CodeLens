using CodeLens.Application.Interfaces.Users;
using CodeLens.Domain.Entites;
using CodeLens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeLens.Infrastructure.Repositories;
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<User?>FindByGitHubIdAsync(long githubId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == githubId);
    }

    public async Task<User>CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User>UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?>FindByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }
}