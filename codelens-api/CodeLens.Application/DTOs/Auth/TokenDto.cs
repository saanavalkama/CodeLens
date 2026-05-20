namespace CodeLens.Application.DTOs.Auth;

public record TokenDto(
    string AccessToken,
    string RefreshToken,
    string RefreshTokenHash
);