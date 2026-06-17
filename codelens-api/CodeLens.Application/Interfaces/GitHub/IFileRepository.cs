using CodeLens.Domain.Entites;

namespace CodeLens.Application.Interfaces.GitHub;

public interface IFileRepository
{
   Task UpsertFilesAsync(List<RepositoryFile> newFiles);
   Task <List<RepositoryFile>> GetFilesByRepoId(Guid repoId);

   Task DeleteBatch(List<RepositoryFile> files);
}