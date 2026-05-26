using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters;
using System.Text.Json;
using CodeLens.Application.DTOs.Auth;
using CodeLens.Application.DTOs.GitHub;
using CodeLens.Application.DTOs.User;
using CodeLens.Application.Interfaces.Auth;
using CodeLens.Application.Interfaces.GitHub;
using CodeLens.Application.Interfaces.Users;
using CodeLens.Application.Interfaces.Utils;
using CodeLens.Domain.Entites;
using CodeLens.Domain.Enums;
using CodeLens.Domain.Exceptions;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace CodeLens.Infrastructure.Services;

public class GitHubService : IGitHubService {
    
    private readonly HttpClient _client;
    private readonly IUserRepository _userRepo;

    private readonly IGitHubAuthService _gitHubAuthService;

    private readonly IHashingService _hasher;

    private readonly IRepoRepository _repoRepo;

    private readonly IFileRepository _fileRepo;

    private readonly IConnectionMultiplexer _redis;


    public GitHubService(
        HttpClient client,
        IUserRepository userRepo,
        IGitHubAuthService gitHubAuthService,
        IHashingService hasher,
        IRepoRepository repoRepo,
        IFileRepository fileRepo,
        IConnectionMultiplexer redis
    )
    {
        _client = client;
        _userRepo = userRepo;
        _gitHubAuthService = gitHubAuthService;
        _hasher = hasher;
        _repoRepo = repoRepo;
        _fileRepo = fileRepo;
        _redis = redis;
        
    }

    public async Task<List<RepoDto>> FetchAndReturnReposAsync(Guid userId)
    {
        //fetch fresh tokens for user
        //tighten logic, use expires at so no extra reqs are made
        var tokens = await RefreshTokens(userId);
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
            UserId= userId
        }).ToList();

        var savedRepos = await _repoRepo.SaveRepositoriesAsync(repos);

        return [.. savedRepos.Select(r => new RepoDto(Id:r.Id,Name:r.Name))];
    }

    public async Task<List<RepoDto>> GetUserReposAsync(Guid userId)
    {
        var repos = await _repoRepo.GetReposByUserIdAsync(userId);
        return [..repos.Select(r => new RepoDto(Id:r.Id, Name:r.Name))];
    }

    public async Task<IndexDto>IndexRepoAsync(Guid repoId, Guid userId)
    {
        var repo = await _repoRepo.GetRepoById(repoId)
            ?? throw new NotFoundException("Repository");

        if(repo.UserId != userId) throw new ForbiddenException("Access denied");

        var tokens = await RefreshTokens(userId);

        var parts = repo.FullName.Split('/');
        var owner = parts[0];  
        var name = parts[1];
        var url = $"https://api.github.com/repos/{owner}/{name}/git/trees/{repo.DefaultBranch}?recursive=1";

        var request = new HttpRequestMessage(HttpMethod.Get,url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("CodeLens", "1.0"));

        var res = await _client.SendAsync(request);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);
        var tree = result.GetProperty("tree");

        var truncated = result.GetProperty("truncated").GetBoolean();
        repo.IsTruncated = truncated;
        await _repoRepo.UpdateAsync(repo);

        var files = tree.EnumerateArray().Select(f => new RepositoryFile
        {
            RepositoryId = repo.Id,
            Path = f.GetProperty("path").GetString() ?? string.Empty,
            Sha = f.GetProperty("sha").GetString() ?? string.Empty,
            Type = f.GetProperty("type").GetString() ?? string.Empty,
        }).ToList();

        await _fileRepo.UpsertFilesAsync(files);

        var db = _redis.GetDatabase();
        await db.StreamAddAsync("indexing-jobs", [
            new NameValueEntry("repoId", repo.Id.ToString()),
            new NameValueEntry("userId", userId.ToString())
        ]);

        return new IndexDto(
            RepoId:repo.Id,
            IndexingStatus: repo.IndexingStatus.ToString(),
            Files: [..files.Select(f => new FileDto(f.Path,f.Type))]
        );
    }

    private async Task<GitHubTokenDto> RefreshTokens(Guid userId)
    {
        //fetch fresh tokens for user
        //tighten logic, use expires at so no extra reqs are made
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

        return tokens;
    }

    public async Task<IndexDto>GetFilesByRepoIdAsync(Guid repoId, Guid userId)
    {
        var repo = await _repoRepo.GetRepoById(repoId) ?? throw new NotFoundException("Repository");

        if(repo.UserId != userId) throw new ForbiddenException("Access denied");

        var files = await _fileRepo.GetFilesByRepoId(repoId);
        return new IndexDto(
            RepoId:repoId,
            IndexingStatus:repo.IndexingStatus.ToString(),
            Files: [..files.Select(f => new FileDto(f.Path,f.Type))]
        );
    }

    
}