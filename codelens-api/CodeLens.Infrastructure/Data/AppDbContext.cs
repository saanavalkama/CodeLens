using Microsoft.EntityFrameworkCore;

namespace CodeLens.Infrastructure.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options){}

    
}