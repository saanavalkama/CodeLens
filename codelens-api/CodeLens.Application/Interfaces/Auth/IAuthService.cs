using CodeLens.Application.DTOs.Auth;
using CodeLens.Application.DTOs.User;

namespace CodeLens.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> HandleGitHubCallbackAsync(JwtDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);

    Task LogoutAsync(string rt);
}