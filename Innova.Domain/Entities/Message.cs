namespace Innova.Domain.Entities;

public class Message : BaseEntity
{
    public int ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public bool IsRead { get; set; } = false;
    
    public Conversation Conversation { get; set; } = null!;
}
