namespace Innova.Application.DTOs.Messaging;

public class ConversationDetailDto
{
    public int Id { get; set; }
    public string OtherParticipantId { get; set; } = string.Empty;
    public string OtherParticipantName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
    public int TotalMessages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}
