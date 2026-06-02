using CodeLens.Domain.Enums;

namespace CodeLens.Domain.Entities;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
    
    public Role Role { get; set; } 
    
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}