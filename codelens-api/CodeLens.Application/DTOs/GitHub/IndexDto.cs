public record IndexDto(
    Guid RepoId,
    string IndexingStatus,
    List<FileDto> Files
);