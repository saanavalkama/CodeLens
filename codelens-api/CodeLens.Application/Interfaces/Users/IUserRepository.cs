using CodeLens.Domain.Entites;

namespace CodeLens.Application.Interfaces.Users;

public interface IUserRepository
{
    Task<User?>FindByGitHubIdAsync(long githubId);
    Task<User>CreateAsync(User user);
    Task<User>UpdateAsync(User user); 
}