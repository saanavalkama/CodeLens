using System.Net.Http.Headers;
using System.Text.Json;
using CodeLens.Application.DTOs.GitHub;
using CodeLens.Application.Interfaces.Auth;
using CodeLens.Application.Interfaces.GitHub;
using CodeLens.Application.Interfaces.Users;
using CodeLens.Application.Interfaces.Utils;
using CodeLens.Domain.Entites;
using CodeLens.Domain.Enums;
using CodeLens.Domain.Exceptions;

namespace CodeLens.Infrastructure.Services;

public class GitHubService : IGitHubService {
    
    private readonly HttpClient _client;
    private readonly IUserRepository _userRepo;

    private readonly IGitHubAuthService _gitHubAuthService;

    private readonly IHashingService _hasher;

    private readonly IRepoRepository _repoRepo;


    public GitHubService(
        HttpClient client,
        IUserRepository userRepo,
        IGitHubAuthService gitHubAuthService,
        IHashingService hasher,
        IRepoRepository repoRepo,
        IUserRepository userRepos
    )
    {
        _client = client;
        _userRepo = userRepo;
        _gitHubAuthService = gitHubAuthService;
        _hasher = hasher;
        _repoRepo = repoRepo;
        
    }

    public async Task<List<RepoDto>> FetchAndReturnReposAsync(Guid userId)
    {
        //fetch fresh tokens for user
        var user = await _userRepo.FindByIdAsync(userId)
        ?? throw new NotFoundException("User");
        var token = user.GitHubRefreshToken ?? throw new NotFoundException("GitHub refresh token");
        var decrypted = _hasher.AES_Decrypt(token);
        var tokens = await _gitHubAuthService.GitHubRefreshAsync(decrypted);
       
       //save tokens
        user.GitHubAccessToken = _hasher.AES_Encrypt(tokens.AccessToken);
        user.GitHubRefreshToken = _hasher.AES_Encrypt(tokens.RefreshToken!);
        user.TokenExpiresAt= tokens.ExpiresAt;
        await _userRepo.UpdateAsync(user);

       //fetch repos
       var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/repos?per_page=100");
       request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",tokens.AccessToken);
       request.Headers.UserAgent.Add(new ProductInfoHeaderValue("CodeLens", "1.0"));

        var res = await _client.SendAsync(request);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement[]>(json) ?? [];

        var repos = result.Select(r => new Repository
        {
            GitHubRepoId =  r.GetProperty("id").GetInt64(),
            Name = r.GetProperty("name").GetString() ?? string.Empty,
            FullName = r.GetProperty("full_name").GetString() ?? string.Empty,
            Description = r.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            IsPrivate = r.GetProperty("private").GetBoolean(),
            DefaultBranch = r.GetProperty("default_branch").GetString() ?? string.Empty,
            UserId= user.Id
        }).ToList();

        var savedRepos = await _repoRepo.SaveRepositoriesAsync(repos);

        return [.. savedRepos.Select(r => new RepoDto(Id:r.Id,Name:r.Name))];
    }

    public async Task<List<RepoDto>> GetUserReposAsync(Guid userId)
    {
        var repos = await _repoRepo.GetReposByUserIdAsync(userId);
        return [..repos.Select(r => new RepoDto(Id:r.Id, Name:r.Name))];
    }
}