using CodeLens.Domain.Entites.Auth;

public interface IRefreshTokenRepository
{
    Task SaveAsync(RefreshToken rt);
    Task<RefreshToken?>GetByTokenHashAsync(string hash);
    Task RevokeAllByFamilyIdAsync(Guid familyId);
    Task RevokeAsync(RefreshToken refreshToken);
}