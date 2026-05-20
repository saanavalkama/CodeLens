using CodeLens.Application.DTOs.User;

namespace CodeLens.Application.Interfaces.Auth;
public interface IGitHubAuthService
{
    string GetAuthorizationUrl();
    Task<GitHubTokenDto> ExchangeCodeForTokenAsync(string code);
    Task<GitHubUserDto> GetUserAsync(GitHubTokenDto dto);
}