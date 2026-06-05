using System.Reflection.Metadata;
using CodeLens.Application.Interfaces.Chat;
using CodeLens.Application.Interfaces.GitHub;
using CodeLens.Domain.Entities;
using CodeLens.Domain.Exceptions;

namespace CodeLens.Application.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _repo;

    private readonly IRepoRepository _repoRepository;

    public ConversationService(
        IConversationRepository repo,
        IRepoRepository repoRepository
    )
    {
        _repo = repo;
        _repoRepository = repoRepository;
    }

    public async Task <ConversationResponseDto> CreateAsync(Guid userId, Guid repoId)
    {
        var access = await ValidateRepoAccess(userId,repoId);
        if(access == false) throw new UnauthorizedException("No access to repository");

        var conversation = new Conversation
        {
            UserId = userId,
            RepositoryId= repoId
        };

        var savedConversation = await _repo.CreateAsync(conversation);

        return new ConversationResponseDto(
            savedConversation.Id, 
            savedConversation.UserId,
            savedConversation.RepositoryId,
            savedConversation.Title,
            savedConversation.CreatedAt
        );
    }

    public async Task<List<ConversationResponseDto>>GetConversationsByRepoIdAsync(Guid userId, Guid repoId)
    {
        var access = await ValidateRepoAccess(userId, repoId);
        if(access == false) throw new UnauthorizedException("No access to repo");

        var conversations = await _repo.GetByRepoIdAsync(repoId);

        return [..conversations.Select(item => new ConversationResponseDto(
            item.Id,
            item.UserId,
            item.RepositoryId,
            item.Title,
            item.CreatedAt
        ))];


    }

    private async Task <bool> ValidateRepoAccess(Guid userId, Guid repoId)
    {
        var repo = await _repoRepository.GetRepoById(repoId) ?? throw new NotFoundException("Repository");
        if(repo.UserId != userId) throw new UnauthorizedException("You don't have access to this repository");
        return true;
    }

}