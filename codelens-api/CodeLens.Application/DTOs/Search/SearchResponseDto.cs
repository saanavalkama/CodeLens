namespace CodeLens.Application.DTOs.Search;

public record ChunkDto(
    string FilePath,
    string Content,
    int StartLine,
    int EndLine,
    float Similarity

);

public record TokenUsageDto(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens
);

public record SearchResponseDto(
    string Answer,
    List<ChunkDto>Chunks,
    TokenUsageDto Usage
    
);