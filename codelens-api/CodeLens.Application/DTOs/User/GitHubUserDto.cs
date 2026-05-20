namespace CodeLens.Application.DTOs.User;

public record GitHubUserDto(
    long Id,
    string Login,
    string? Email,
    string AvatarUrl,
    string AccessToken,
    string? RefreshToken,
    DateTime? TokenExpiresAt
);