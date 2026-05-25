using CodeLens.Application.Interfaces.GitHub;
using CodeLens.Domain.Entites;
using CodeLens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace CodeLens.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly AppDbContext _context;

    public FileRepository(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task UpsertFilesAsync(List<RepositoryFile> newFiles)
    {
    foreach (var file in newFiles)
    {
        var existing = await _context.RepositoryFiles
            .FirstOrDefaultAsync(f => f.RepositoryId == file.RepositoryId 
                                   && f.Path == file.Path);
        if (existing == null)
        {
            await _context.RepositoryFiles.AddAsync(file);
        }
        else
        {
            // only reset if file has changed
            if (existing.Sha != file.Sha)
            {
                existing.Sha = file.Sha;
                existing.IndexingStatus = Domain.Enums.IndexingStatus.Pending;
                existing.IndexedAt = null;
                existing.IndexingError = null;
            }
        }
    }

        await _context.SaveChangesAsync();
    }
}