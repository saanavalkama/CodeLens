using CodeLens.Application.DTOs.Chat;

namespace CodeLens.Application.DTOs.Search;

public record SearchRequestDto(
    string Query,
    Guid RepoId,
    List<MessageDto> History,
    Guid UserId
);