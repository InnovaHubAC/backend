namespace Innova.Application.DTOs.Messaging;

public class SendMessageDto
{
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
