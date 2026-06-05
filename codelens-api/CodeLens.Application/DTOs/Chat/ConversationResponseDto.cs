public record ConversationResponseDto(
    Guid Id, 
    Guid UserId, 
    Guid RepoId, 
    string Title,
    DateTime CreatedAt
);