namespace CodeLens.Domain.Entites.Graph;

public class GraphEdge
{
    public Guid Id {get;set;} = Guid.NewGuid();

    public Guid RepositoryId {get;set;}

    public Repository Repository {get;set;} = null!;

    public Guid SourceId {get;set;}

    public Guid TargetId {get;set;}

    public string EdgeType {get;set;} = string.Empty;

    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;

    public GraphNode Source {get;set;} = null!;

    public GraphNode Target {get;set;} = null!;
}