namespace Innova.Application.DTOs.Notification;

public class NotificationDto
{
    public int Id { get; set; }
    public string RecipientId { get; set; } = string.Empty;
    public string? TriggeredById { get; set; }
    public NotificationType NotificationType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public int? IdeaId { get; set; }
    public int? VoteId { get; set; }
}
