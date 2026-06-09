using System.Data.Common;
using CodeLens.Application.DTOs;
using CodeLens.Application.DTOs.User;
using CodeLens.Application.Interfaces.Users;
using CodeLens.Application.Interfaces.Utils;
using CodeLens.Domain.Entites;
using CodeLens.Domain.Enums;
using CodeLens.Domain.Exceptions;

namespace CodeLens.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IHashingService _hasher;
    public UserService(
        IUserRepository repo,
        IHashingService hasher
    )
    {
        _repo = repo;
        _hasher = hasher;
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
                GitHubAccessToken = _hasher.AES_Encrypt(dto.AccessToken),
                GitHubRefreshToken = dto.RefreshToken != null ? _hasher.AES_Encrypt(dto.RefreshToken) : null,
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
            existingUser.GitHubAccessToken = _hasher.AES_Encrypt(dto.AccessToken);
            existingUser.GitHubRefreshToken = dto.RefreshToken != null ? _hasher.AES_Encrypt(dto.RefreshToken) : null;
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

    public async Task<MeResponseDto>Me(Guid id)
    {
        var user = await _repo.FindByIdAsync(id) ?? throw new  NotFoundException("User");

        return new MeResponseDto(
            Id:user.Id,
            GithubUsername: user.GitHubUsername,
            UserTier: user.UserTier.ToString()
        );
    }

    public async Task CreateOrUpdateKeyAsync(Guid userId, string key)
    {
        var user = await _repo.FindByIdAsync(userId)
            ?? throw new NotFoundException("User");

        user.EncryptedOpenAIKey = _hasher.AES_Encrypt(key);
        await _repo.UpdateAsync(user);
        
    }

    public async Task <bool> KeyStatusAsync(Guid userId)
    {
        var user = await _repo.FindByIdAsync(userId) 
            ?? throw new NotFoundException("User");
        return !string.IsNullOrEmpty(user.EncryptedOpenAIKey);
    }
}