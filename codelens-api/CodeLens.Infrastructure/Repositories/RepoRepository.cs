using CodeLens.Application.Interfaces.GitHub;
using CodeLens.Domain.Entites;
using CodeLens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class RepoRepository : IRepoRepository
{
    private readonly AppDbContext _context;

    public RepoRepository(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<List<Repository>> SaveRepositoriesAsync(List<Repository> repos)
    {
      foreach (var repo in repos)
    {
        var existing = await _context.Repositories
            .FirstOrDefaultAsync(r => r.GitHubRepoId == repo.GitHubRepoId && r.UserId == repo.UserId);

        if (existing == null)
        {
            await _context.Repositories.AddAsync(repo);
        }
        else
        {
            existing.Name = repo.Name;
            existing.FullName = repo.FullName;
            existing.Description = repo.Description;
            existing.IsPrivate = repo.IsPrivate;
            existing.DefaultBranch = repo.DefaultBranch;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }

        await _context.SaveChangesAsync();
        return repos;
    }

     public async Task<List<Repository>> GetReposByUserIdAsync(Guid userId)
    {
        var repos =  await _context.Repositories
            .Where(r => r.UserId == userId)
            .ToListAsync();
        return repos;
    }

    public async Task<Repository?> GetRepoById(Guid id)
    {
        return await _context.Repositories
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Repository>UpdateAsync(Repository repo)
    {
        _context.Repositories.Update(repo);
        await _context.SaveChangesAsync();
        return repo;

    }


    
}