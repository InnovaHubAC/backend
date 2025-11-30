namespace Innova.Application.DTOs.Messaging;

public class ConversationDto
{
    public int Id { get; set; }
    public string OtherParticipantId { get; set; } = string.Empty;
    public string OtherParticipantName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public MessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}
