namespace CodeLens.Domain.Entites.Auth;

public class RefreshToken
{
    public Guid Id {get;set;} = Guid.NewGuid();

    public Guid FamilyId {get;set;} = Guid.NewGuid();
   
    public string TokenHash {get;set;} = string.Empty;

    public DateTime ExpiresAt {get;set;} = DateTime.UtcNow.AddDays(7);

    public DateTime? RevokedAt {get;set;}

    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;

    public Guid UserId {get;set;}
    public User User {get;set;} = null!;


}