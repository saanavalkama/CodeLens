namespace CodeLens.Application.DTOs.User;

public record GitHubTokenDto(
    string AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAt
);