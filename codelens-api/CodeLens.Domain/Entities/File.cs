using CodeLens.Domain.Enums;

namespace CodeLens.Domain.Entites;

public class RepositoryFile
{
    public Guid Id {get;set;} = Guid.NewGuid();

    public Guid RepositoryId {get;set;}

    public Repository Repository {get;set;} = null!;

    public string Path {get;set;} = string.Empty;

    public string Sha {get;set;} = string.Empty;

    public string Type {get;set;} = string.Empty;

    public IndexingStatus IndexingStatus {get;set;} = IndexingStatus.Pending;

    public string? IndexingError {get;set;}

    public DateTime? IndexedAt {get;set;}

    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;

    public DateTime UpdatedAt {get;set;} = DateTime.UtcNow;

}