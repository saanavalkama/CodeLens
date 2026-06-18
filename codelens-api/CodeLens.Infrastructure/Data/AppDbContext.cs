using CodeLens.Domain.Entites;
using CodeLens.Domain.Entites.Auth;
using CodeLens.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeLens.Infrastructure.Data;

public class AppDbContext: DbContext
{
   public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
   public DbSet<User>Users {get;set;}
   public DbSet<Repository> Repositories {get;set;}
   public DbSet<RefreshToken>RefreshTokens {get;set;}

   public DbSet<RepositoryFile> RepositoryFiles {get;set;}

   public DbSet<Conversation>Conversations {get;set;}

   public DbSet<Message>Messages {get;set;}

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
            entity.HasIndex(f => f.RepositoryId);
            entity
                .HasOne(f => f.Repository)
                .WithMany()
                .HasForeignKey(f => f.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);   
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.RepositoryId);
            entity
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(c => c.Repository)
                .WithMany()
                .HasForeignKey(c => c.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.ConversationId);
            entity
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }


}