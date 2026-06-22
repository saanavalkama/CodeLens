namespace CodeLens.Domain.Entites.Graph;

public class FileContent
{
    public Guid Id {get;set;} = Guid.NewGuid();

    public Guid RepositoryFileId {get;set;}

    public RepositoryFile RepositoryFile {get;set;} = null!;

    public string Content {get;set;} = string.Empty;

    DateTime CreatedAt {get;set;} = DateTime.UtcNow;
}