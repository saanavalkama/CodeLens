namespace CodeLens.Application.DTOs.User;

public record MeResponseDto
(
    Guid Id,
    string GithubUsername,
    string UserTier
);