using CodeLens.Application.DTOs;
using CodeLens.Application.DTOs.User;
using CodeLens.Domain.Entites;

namespace CodeLens.Application.Interfaces.Users;

public interface IUserService
{
    Task<JwtDto>FindOrCreateAsync(GitHubUserDto gitHubUserDto);
    Task<MeResponseDto>Me(Guid Id);

    Task CreateOrUpdateKeyAsync(Guid userId, string key);

    Task <bool> KeyStatusAsync(Guid userId);
}