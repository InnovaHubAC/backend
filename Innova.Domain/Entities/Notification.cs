namespace Innova.Domain.Entities;

public class Notification : BaseEntity
{
    public string RecipientId { get; set; } = string.Empty;
    public string? TriggeredById { get; set; }
    public NotificationType NotificationType { get; set; } = NotificationType.Unknown;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public int? IdeaId { get; set; }
    public Idea? Idea { get; set; }
    public int? VoteId { get; set; }
    public Vote? Vote { get; set; }
}
