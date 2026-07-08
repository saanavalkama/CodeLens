using CodeLens.Domain.Enums;

namespace CodeLens.Domain.Entites;

public class Repository
{
    public Guid Id {get;set;} = Guid.NewGuid();

    public long GitHubRepoId {get;set;}

    public string Name {get;set;} = string.Empty;

    public string FullName {get;set;} = string.Empty;

    public string? Description {get;set;}

    public bool IsPrivate {get;set;}
    public string DefaultBranch {get;set;} = string.Empty;

    public IndexingStatus IndexingStatus {get;set;} = IndexingStatus.Pending;

    public DateTime? LastIndexedAt {get;set;}

    public string? IndexingError {get;set;}

    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;

    public DateTime UpdatedAt {get;set;} = DateTime.UtcNow;

    public Guid UserId {get;set;}

    public User User {get;set;} = null!;

    public bool IsTruncated {get;set;} = false;

    public int TotalFiles {get;set;}

    public int IndexedFiles {get;set;}

    public DateTime? LastProgressAt {get;set;}

    public string? Summary { get; set; }

    public DateTime? SummaryGeneratedAt { get; set; }

}