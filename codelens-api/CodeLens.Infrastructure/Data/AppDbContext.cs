using CodeLens.Domain.Entites;
using CodeLens.Domain.Entites.Auth;
using CodeLens.Domain.Entites.Graph;
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

   public DbSet<FileChunk>Chunks{get;set;}

   public DbSet<GraphNode> GraphNodes { get; set; }
    public DbSet<GraphEdge> GraphEdges { get; set; }
    public DbSet<FileContent> FileContents { get; set; }

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

        modelBuilder.Entity<FileChunk>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.ToTable("FileChunks");
            entity
                .HasOne<RepositoryFile>()
                .WithMany()
                .HasForeignKey(c => c.RepositoryFileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GraphNode>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.ToTable("GraphNodes");
            entity.HasIndex(g => new {g.RepositoryId, g.Name});
            entity.HasIndex(g => g.RepositoryId);
            entity.HasIndex(n => n.FilePath);
            entity
                .HasOne(g => g.Repository)
                .WithMany()
                .HasForeignKey(g => g.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GraphEdge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("GraphEdges");
            entity.HasIndex(e => new {e.RepositoryId, e.SouceId});
            entity.HasIndex(e => new {e.RepositoryId, e.TargetId});
            entity
                .HasOne(e => e.Source)
                .WithMany(n => n.OutgoingEdges)
                .HasForeignKey(e => e.SouceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(e => e.Target)
                .WithMany(n => n.IncomingEdges)
                .HasForeignKey(e=>e.TargetId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<FileContent>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.ToTable("FileContents");
            entity.HasIndex( f => f.RepositoryFileId).IsUnique();
            entity
                .HasOne(f => f.RepositoryFile)
                .WithMany()
                .HasForeignKey(f => f.RepositoryFileId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        

    }


}