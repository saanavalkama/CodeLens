
using CodeLens.Domain.Entites.Auth;
using CodeLens.Domain.Enums;

namespace CodeLens.Domain.Entites;
public class User
{
    public Guid Id {get;set;} = Guid.NewGuid();
    public long GitHubId {get;set;}

    public string GitHubUsername {get;set;} = string.Empty;

    public string? Email {get;set;}

    public string? AvatarUrl {get;set;}

    public string GitHubAccessToken {get;set;} = string.Empty;

    public string? GitHubRefreshToken {get;set;}

    public DateTime? TokenExpiresAt {get;set;}

    public UserTier UserTier {get;set;} = UserTier.Free;

    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get;set;} = DateTime.UtcNow;

    public ICollection<Repository> Repositories {get;set;} = new List<Repository>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public string? EncryptedOpenAIKey {get;set;}
}