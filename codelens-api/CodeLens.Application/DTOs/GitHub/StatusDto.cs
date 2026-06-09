namespace CodeLens.Application.DTOs.GitHub;

public record RepoStatusDto(
    Guid Id,
    string Status,
    int TotalFiles,
    int IndexedFiles,
    int PercentComplete
);