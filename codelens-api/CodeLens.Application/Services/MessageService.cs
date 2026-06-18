using CodeLens.Application.DTOs.Chat;
using CodeLens.Application.DTOs.Search;
using CodeLens.Application.Interfaces.Chat;
using CodeLens.Application.Interfaces.Search;
using CodeLens.Domain.Entities;
using CodeLens.Domain.Exceptions;

namespace CodeLens.Application.Services;

public class MessageService : IMessageService
{
    private readonly IFastApiClient _client;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    public MessageService(
        IFastApiClient client,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository
    )
    {
        _client = client;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
    }

    public async Task<SearchResponseDto>SearchAsync(Guid userId, Guid repoId, Guid conversationId, string message, CancellationToken ct)
    {
       var conversation = await _conversationRepository.GetByIdAsync(conversationId)
            ?? throw new NotFoundException("Conversation");
       if(conversation.UserId != userId) throw new UnauthorizedException("No access to this resource");

       var history = await _messageRepository.GetNByConversationIdAsync(conversationId, 10);

       var requestDto = new SearchRequestDto(
        Query: message,
        RepoId: repoId,
        History: history.Select(m => new MessageDto(m.Role.ToString(), m.Content)).ToList(),
        UserId: userId
       );

        var userMessage = new Message
        {
            ConversationId = conversationId,
            Role = "user",
            Content = message
        };

       await _messageRepository.SaveMessageAsync(userMessage);

       var searchResponseDto = await _client.SearchAsync(requestDto, ct);
    
       var assistantMessage = new Message
       {
           ConversationId = conversationId,
           Role = "assistant",
           Content = searchResponseDto.Answer
       };

       await _messageRepository.SaveMessageAsync(assistantMessage);

       conversation.UpdatedAt = DateTime.UtcNow;

       await _conversationRepository.UpdateAsync(conversation);

        return searchResponseDto;
    }

    public async Task <List<MessageDto>>GetNMessagesAync(Guid userId, Guid repoId, Guid conversationId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId)
            ?? throw new NotFoundException("Conversation");
       if(conversation.UserId != userId) throw new UnauthorizedException("No access to this resource");

       var history = await _messageRepository.GetNByConversationIdAsync(conversationId, 10);

       return [..history.Select(i => new MessageDto(i.Role, i.Content))];

    }
}