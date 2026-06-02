namespace CodeLens.Application.DTOs.Search;

public record SearchChunkDto(
    string Type,
    string? Content
);