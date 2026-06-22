namespace CodeLens.Domain.Entites.Graph;

public class GraphNode
{
    public Guid Id {get;set;} = Guid.NewGuid();
    public Guid RepositoryId {get;set;}

    public Repository Repository {get;set;} = null!;
    public string NodeType {get;set;} = string.Empty;

    public string Name {get;set;} = string.Empty;

    public string FilePath = string.Empty;

    public string? Signature {get;set;}

    public int? StartLine {get;set;}

    public int? EndLine {get;set;}

    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;

    public ICollection<GraphEdge> OutgoingEdges {get;set;} = new List<GraphEdge>();
    public ICollection<GraphEdge> IncomingEdges {get;set;} = new List<GraphEdge>();


}