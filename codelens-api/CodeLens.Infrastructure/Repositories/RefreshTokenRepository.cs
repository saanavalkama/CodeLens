using CodeLens.Domain.Entites.Auth;
using CodeLens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeLens.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(
        AppDbContext context
    ){
        _context = context;
    }

    public async Task SaveAsync(RefreshToken rt)
    {
        _context.RefreshTokens.Add(rt);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string hash)
    {
        var token = await _context.RefreshTokens
        .Include(t => t.User)
        .FirstOrDefaultAsync(t => t.TokenHash == hash);
        return token; 
    }

    public async Task RevokeAllByFamilyIdAsync(Guid familyId)
    {
        var tokens = await _context.RefreshTokens.Where(t => t.FamilyId == familyId).ToListAsync();
        foreach (var token in tokens){
            token.RevokedAt = DateTime.UtcNow;

        }
        await _context.SaveChangesAsync();
    }
 
    public async Task RevokeAsync(RefreshToken token)
    {
       //gets called after token hash has been found, so item exists for sure
        token.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        

    }
}