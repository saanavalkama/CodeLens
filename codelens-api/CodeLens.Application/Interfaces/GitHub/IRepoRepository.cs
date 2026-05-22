using CodeLens.Domain.Entites;

namespace CodeLens.Application.Interfaces.GitHub;

public interface IRepoRepository
{
    Task <List<Repository>>SaveRepositoriesAsync(List<Repository> repos);
    Task<List<Repository>> GetReposByUserIdAsync(Guid userId);
}