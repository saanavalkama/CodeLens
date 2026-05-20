using System.Data.Common;
using CodeLens.Application.DTOs;
using CodeLens.Application.DTOs.User;
using CodeLens.Application.Interfaces.Users;
using CodeLens.Domain.Entites;

namespace CodeLens.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(
        IUserRepository repo
    )
    {
        _repo = repo;
    }

    public async Task<JwtDto> FindOrCreateAsync(GitHubUserDto dto)
    {
        var existingUser = await _repo.FindByGitHubIdAsync(dto.Id);

        User savedUser;

        if(existingUser == null)
        {
            var newUser = new User
            {
                GitHubId = dto.Id,
                GitHubUsername = dto.Login,
                Email = dto.Email,
                AvatarUrl = dto.AvatarUrl,
                GitHubAccessToken = dto.AccessToken,
                GitHubRefreshToken = dto.RefreshToken,
                TokenExpiresAt= dto.TokenExpiresAt
            };

            savedUser = await _repo.CreateAsync(newUser);

            return new JwtDto(
                Id: savedUser.Id,
                GitHubUsername: savedUser.GitHubUsername,
                UserTier: savedUser.UserTier.ToString()
            );
            
        }
        else
        {
            existingUser.GitHubUsername = dto.Login;
            existingUser.Email = dto.Email;
            existingUser.AvatarUrl = dto.AvatarUrl;
            existingUser.GitHubAccessToken = dto.AccessToken;
            existingUser.GitHubRefreshToken = dto.RefreshToken;
            existingUser.TokenExpiresAt = dto.TokenExpiresAt;
            existingUser.UpdatedAt = DateTime.UtcNow;

            savedUser = await _repo.UpdateAsync(existingUser);

            return new JwtDto(
                Id: savedUser.Id,
                GitHubUsername:savedUser.GitHubUsername,
                UserTier: savedUser.UserTier.ToString()
            );
        }
    }
}