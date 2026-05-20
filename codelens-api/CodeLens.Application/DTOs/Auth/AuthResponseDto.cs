namespace CodeLens.Application.DTOs.Auth;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken
);