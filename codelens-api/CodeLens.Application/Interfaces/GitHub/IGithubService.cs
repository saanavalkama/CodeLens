using CodeLens.Application.DTOs.GitHub;

namespace CodeLens.Application.Interfaces.GitHub;

public interface IGitHubService
{
    Task <List<RepoDto>> FetchAndReturnReposAsync(Guid userId);
    Task <List<RepoDto>> GetUserReposAsync(Guid userId);

    Task<IndexDto>IndexRepoAsync(Guid repoId, Guid userId);

    Task<IndexDto>GetFilesByRepoIdAsync(Guid repoId, Guid userId);

    Task<RepoStatusDto>GetRepoStatusAsync(Guid userId, Guid repoId);

    Task ReIndexRepo(Guid repoId, Guid userId);
}