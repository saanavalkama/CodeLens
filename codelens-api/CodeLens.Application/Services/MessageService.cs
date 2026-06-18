using CodeLens.Application.DTOs.Chat;
using CodeLens.Application.DTOs.Search;
using CodeLens.Application.Interfaces.Chat;
using CodeLens.Application.Interfaces.Search;
using CodeLens.Domain.Entities;
using CodeLens.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CodeLens.Application.Services;

public class MessageService : IMessageService
{
    private readonly IFastApiClient _client;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;

    private readonly ILogger<MessageService> _logger;
    public MessageService(
        IFastApiClient client,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ILogger<MessageService> logger
    )
    {
        _client = client;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task<SearchResponseDto>SearchAsync(Guid userId, Guid repoId, Guid conversationId, string message, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

       var conversation = await _conversationRepository.GetByIdAsync(conversationId)
            ?? throw new NotFoundException("Conversation");
       if(conversation.UserId != userId) throw new UnauthorizedException("No access to this resource");

       if(string.IsNullOrEmpty(conversation.Title))
        {
            conversation.Title = message.Length > 50 ? message[..50] : message;
            await _conversationRepository.UpdateAsync(conversation);
        }

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

       _logger.LogInformation("Search started for repo {RepoId} conversation {ConversationId} query length {QueryLength}",
            repoId, conversationId, message.Length);

        var fastApiSw = Stopwatch.StartNew();

       var searchResponseDto = await _client.SearchAsync(requestDto, ct);

       fastApiSw.Stop();
    
       var assistantMessage = new Message
       {
           ConversationId = conversationId,
           Role = "assistant",
           Content = searchResponseDto.Answer
       };

       await _messageRepository.SaveMessageAsync(assistantMessage);

       conversation.UpdatedAt = DateTime.UtcNow;

       await _conversationRepository.UpdateAsync(conversation);

       sw.Stop();

        _logger.LogInformation(
            "Search completed for repo {RepoId} | total {TotalMs}ms | fastapi {FastApiMs}ms | tokens {TotalTokens}",
            repoId, sw.ElapsedMilliseconds, fastApiSw.ElapsedMilliseconds,
            searchResponseDto.Usage?.TotalTokens ?? 0);

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