namespace CodeLens.Application.DTOs.User;

public record JwtDto
(
    Guid Id,
    string GitHubUsername,
    string UserTier
);