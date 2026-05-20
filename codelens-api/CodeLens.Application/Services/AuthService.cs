using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using CodeLens.Application.DTOs.Auth;
using CodeLens.Application.DTOs.User;
using CodeLens.Application.Interfaces.Auth;
using CodeLens.Application.Utils;
using CodeLens.Domain.Entites.Auth;

namespace CodeLens.Application.Services;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _rtrepo;

    public AuthService(
        IJwtService jwtService,
        IRefreshTokenRepository rtrepo
    )
    {
        _jwtService = jwtService;
        _rtrepo = rtrepo;
    }

    public async Task<AuthResponseDto>HandleGitHubCallbackAsync(JwtDto dto)
    {
    
       var tokenDto = _jwtService.GenerateTokens(dto);

      var rt = new RefreshToken
      {
          TokenHash = tokenDto.RefreshTokenHash,
          FamilyId = Guid.NewGuid(),
          UserId = dto.Id,
          ExpiresAt = DateTime.UtcNow.AddDays(7)
      };

      await _rtrepo.SaveAsync(rt);

      return new AuthResponseDto(
        AccessToken: tokenDto.AccessToken,
        RefreshToken: tokenDto.RefreshToken
      );
    }

    public async Task<AuthResponseDto>RefreshTokenAsync(string rt)
    {
        var hash = Hashers.SHA256_Hasher(rt);

        var existingToken = await _rtrepo.GetByTokenHashAsync(hash);
        if(existingToken == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }
        if(existingToken.RevokedAt != null)
        {
            await _rtrepo.RevokeAllByFamilyIdAsync(existingToken.FamilyId);
            throw new UnauthorizedAccessException("Token reuse detected");
        }
        if(existingToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token is expired");
        }

        await _rtrepo.RevokeAsync(existingToken);

        var jwtDto = new JwtDto(
            Id: existingToken.User.Id,
            GitHubUsername: existingToken.User.GitHubUsername,
            UserTier: existingToken.User.UserTier.ToString()
        );

        var tokenDto = _jwtService.GenerateTokens(jwtDto);

        var newRt = new RefreshToken
        {
            UserId = existingToken.UserId,
            FamilyId = existingToken.FamilyId,
            TokenHash = tokenDto.RefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _rtrepo.SaveAsync(newRt);

        return new AuthResponseDto(
            AccessToken:tokenDto.AccessToken,
            RefreshToken:tokenDto.RefreshToken
        );

    }

    public async Task LogoutAsync(string rt)
    {
        var hash = Hashers.SHA256_Hasher(rt);
        var existingToken = await _rtrepo.GetByTokenHashAsync(hash);

        if(existingToken != null)
        {
            await _rtrepo.RevokeAllByFamilyIdAsync(existingToken.FamilyId);
        }
    }

}