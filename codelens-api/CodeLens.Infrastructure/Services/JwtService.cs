using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CodeLens.Application.DTOs.Auth;
using CodeLens.Application.DTOs.User;
using CodeLens.Application.Interfaces.Auth;
using CodeLens.Application.Interfaces.Utils;
using CodeLens.Application.Utils;
using CodeLens.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CodeLens.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options; 
    private readonly IHashingService _hasher;

    public JwtService(
        IOptions<JwtOptions> options,
        IHashingService hasher
    )
    {
        _options = options.Value;
        _hasher = hasher;
    }

    public string GenerateAccessToken(JwtDto dto)
    {

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, dto.Id.ToString()),
            new Claim(ClaimTypes.Name, dto.GitHubUsername),
            new Claim(ClaimTypes.Role, dto.UserTier)
        };

        var token = new JwtSecurityToken(
            issuer:_options.Issuer,
            audience:_options.Audience,
            claims:claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
        
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public TokenDto GenerateTokens(JwtDto dto)
    {
        var accessToken = GenerateAccessToken(dto);
        var refreshToken = GenerateRefreshToken();
        var hash = _hasher.SHA256_Hasher(refreshToken);

        return new TokenDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            RefreshTokenHash:hash
        );
    }

}