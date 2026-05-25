using CodeLens.Domain.Entites;
using CodeLens.Domain.Entites.Auth;
using Microsoft.EntityFrameworkCore;

namespace CodeLens.Infrastructure.Data;

public class AppDbContext: DbContext
{
   public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
   public DbSet<User>Users {get;set;}
   public DbSet<Repository> Repositories {get;set;}
   public DbSet<RefreshToken>RefreshTokens {get;set;}

   public DbSet<RepositoryFile> RepositoryFiles {get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.GitHubId).IsUnique();
        });

        modelBuilder.Entity<Repository>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.GitHubRepoId).IsUnique();
            entity
              .HasOne(r => r.User)
              .WithMany(u => u.Repositories)
              .HasForeignKey(r => r.UserId)
              .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.TokenHash).IsUnique();
            entity.HasIndex(r => r.FamilyId);
            entity
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);  
        });

        modelBuilder.Entity<RepositoryFile>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => new {f.RepositoryId, f.Path});
            entity
                .HasOne(f => f.Repository)
                .WithMany()
                .HasForeignKey(f => f.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }


}