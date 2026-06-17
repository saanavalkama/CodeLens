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
using CodeLens.Domain.Constants;

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

        if (repo.IndexingStatus == IndexingStatus.Indexing || repo.IndexingStatus == IndexingStatus.Completed)
        {
            var existingFiles = await _fileRepo.GetFilesByRepoId(repoId);
            return new IndexDto(
                RepoId: repo.Id,
                IndexingStatus: repo.IndexingStatus.ToString(),
                Files: [..existingFiles.Select(f => new FileDto(f.Path, f.Type))]
            );
        }

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

        var indexableFiles = tree.EnumerateArray()
            .Where(f => (f.GetProperty("type").GetString() ?? string.Empty) == "blob"
                     && IndexingConstants.IsIndexable(f.GetProperty("path").GetString() ?? string.Empty))
            .Select(f => new RepositoryFile
            {
                RepositoryId = repo.Id,
                Path = f.GetProperty("path").GetString() ?? string.Empty,
                Sha = f.GetProperty("sha").GetString() ?? string.Empty,
                Type = f.GetProperty("type").GetString() ?? string.Empty,
            }).ToList();

        await _fileRepo.UpsertFilesAsync(indexableFiles);

        repo.IndexingStatus = IndexingStatus.Indexing;
        repo.TotalFiles = indexableFiles.Count;
        repo.IndexedFiles = 0;
        await _repoRepo.UpdateAsync(repo);

        var db = _redis.GetDatabase();
        await db.StreamAddAsync("indexing-jobs", [
            new NameValueEntry("repoId", repo.Id.ToString()),
            new NameValueEntry("userId", userId.ToString())
        ]);

        return new IndexDto(
            RepoId:repo.Id,
            IndexingStatus: repo.IndexingStatus.ToString(),
            Files: [..indexableFiles.Select(f => new FileDto(f.Path,f.Type))]
        );
    }

    private async Task<GitHubTokenDto> RefreshTokens(Guid userId)
    {
        var user = await _userRepo.FindByIdAsync(userId)
        ?? throw new NotFoundException("User");

        var accessToken = _hasher.AES_Decrypt(user.GitHubAccessToken);

        // Only call GitHub's refresh endpoint if the token has an expiry and is about to expire
        if (!user.TokenExpiresAt.HasValue || user.TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
            return new GitHubTokenDto(AccessToken: accessToken, RefreshToken: null, ExpiresAt: user.TokenExpiresAt);

        var refreshToken = user.GitHubRefreshToken ?? throw new NotFoundException("GitHub refresh token");
        var decryptedRefresh = _hasher.AES_Decrypt(refreshToken);
        var tokens = await _gitHubAuthService.GitHubRefreshAsync(decryptedRefresh);

        user.GitHubAccessToken = _hasher.AES_Encrypt(tokens.AccessToken);
        if (tokens.RefreshToken != null)
            user.GitHubRefreshToken = _hasher.AES_Encrypt(tokens.RefreshToken);
        user.TokenExpiresAt = tokens.ExpiresAt;
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

    public async Task<RepoStatusDto>GetRepoStatusAsync(Guid userId, Guid repoId)
    {
        var repo = await _repoRepo.GetRepoById(repoId) ?? throw new NotFoundException("Repo");
        if(repo.UserId != userId) throw new ForbiddenException("No access to this repo");

        if (repo.IndexingStatus == IndexingStatus.Indexing)
        {
            var lastActivity = repo.LastProgressAt ?? repo.UpdatedAt;
            if (lastActivity < DateTime.UtcNow.AddMinutes(-15))
            {
                repo.IndexingStatus = IndexingStatus.Failed;
                await _repoRepo.UpdateAsync(repo);
            }
        }

        return new RepoStatusDto(
            repo.Id,
            repo.IndexingStatus.ToString(),
            repo.TotalFiles,
            repo.IndexedFiles,
            repo.TotalFiles == 0 ? 0 : (repo.IndexedFiles * 100) / repo.TotalFiles,
            repo.LastProgressAt
        );
    }

    public async Task ReIndexRepo(Guid repoId, Guid userId)
    {
        var repo = await _repoRepo.GetRepoById(repoId)
            ?? throw new NotFoundException("Repository");

        if (repo.UserId != userId) throw new ForbiddenException("No permission to re-index this repo");

        if(repo.IndexingStatus == IndexingStatus.Indexing) return;

        var user = await _userRepo.FindByIdAsync(userId)
            ?? throw new NotFoundException("User");

        string token = user.TokenExpiresAt <= DateTime.UtcNow.AddMinutes(5) 
        ? (await RefreshTokens(userId)).AccessToken
        : _hasher.AES_Decrypt(user.GitHubAccessToken);


        var parts = repo.FullName.Split('/');
        var owner = parts[0];  
        var name = parts[1];
        var url = $"https://api.github.com/repos/{owner}/{name}/git/trees/{repo.DefaultBranch}?recursive=1";

        var request = new HttpRequestMessage(HttpMethod.Get,url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("CodeLens", "1.0"));

        var res = await _client.SendAsync(request);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);
        var tree = result.GetProperty("tree");

        var truncated = result.GetProperty("truncated").GetBoolean();
        repo.IsTruncated = truncated;
        await _repoRepo.UpdateAsync(repo);

        var indexableGithubFiles = tree.EnumerateArray()
            .Where(f => (f.GetProperty("type").GetString() ?? string.Empty) == "blob"
                     && IndexingConstants.IsIndexable(f.GetProperty("path").GetString() ?? string.Empty))
            .Select(f => new RepositoryFile
            {
                RepositoryId = repo.Id,
                Path = f.GetProperty("path").GetString() ?? string.Empty,
                Sha = f.GetProperty("sha").GetString() ?? string.Empty,
                Type = f.GetProperty("type").GetString() ?? string.Empty,
            }).ToList();

        var githubFilesByPath = indexableGithubFiles.ToDictionary(f => f.Path, f=>f);

        var storedFiles = await _fileRepo.GetFilesByRepoId(repoId);

        var storedByPath = storedFiles.ToDictionary(f => f.Path, f=>f);

        var changed = storedByPath
            .Where(s => githubFilesByPath.ContainsKey(s.Key) && githubFilesByPath[s.Key].Sha != s.Value.Sha)
            .Select(s => s.Value)
            .ToList();

        //new files default for pending
        var added = githubFilesByPath
            .Where(g => !storedByPath.ContainsKey(g.Key))
            .Select(g => g.Value)
            .ToList();

        var deleted = storedByPath
            .Where(s => !githubFilesByPath.ContainsKey(s.Key))
            .Select(s => s.Value)
            .ToList();

        var unchanged = storedByPath
            .Where(s => githubFilesByPath.ContainsKey(s.Key) && s.Value.Sha == githubFilesByPath[s.Key].Sha)
            .Count();

        //add new files - status pending
        await _fileRepo.UpsertFilesAsync(added);

        //changed files - mark them as pending, delete chunks and save
        await _fileRepo.DeleteBatch(changed);
        
        changed.ForEach(f =>
        {
            f.IndexingStatus = IndexingStatus.Pending;
            f.Sha = githubFilesByPath[f.Path].Sha;
            f.UpdatedAt = DateTime.UtcNow;
        });
        
        await _fileRepo.UpsertFilesAsync(changed);

        //deleted files
        await _fileRepo.DeleteBatch(deleted);

        repo.TotalFiles = changed.Count + unchanged + added.Count;
        repo.IndexedFiles = unchanged;
        repo.UpdatedAt = DateTime.UtcNow;
        repo.IndexingStatus = IndexingStatus.Indexing;
        await _repoRepo.UpdateAsync(repo);


        var db = _redis.GetDatabase();
        await db.StreamAddAsync("indexing-jobs", [
            new NameValueEntry("repoId", repo.Id.ToString()),
            new NameValueEntry("userId", userId.ToString())
        ]);


    }

    
}