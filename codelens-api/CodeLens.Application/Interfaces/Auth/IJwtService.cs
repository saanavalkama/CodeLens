using System.Runtime.CompilerServices;
using CodeLens.Application.DTOs.Auth;
using CodeLens.Application.DTOs.User;

namespace CodeLens.Application.Interfaces.Auth;

public interface IJwtService
{
    public string GenerateAccessToken(JwtDto dto);
    public string GenerateRefreshToken();

    public TokenDto GenerateTokens(JwtDto dto);

}