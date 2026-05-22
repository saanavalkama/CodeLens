using CodeLens.Application.DTOs.GitHub;

namespace CodeLens.Application.Interfaces.GitHub;

public interface IGitHubService
{
    Task <List<RepoDto>> FetchAndReturnReposAsync(Guid userId);
    Task <List<RepoDto>> GetUserReposAsync(Guid userId);
}