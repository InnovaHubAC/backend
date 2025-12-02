namespace Innova.Domain.Entities;

public class Conversation : BaseEntity
{
    public string ParticipantOneId { get; set; } = string.Empty;
    public string ParticipantTwoId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastMessageAt { get; set; }
    
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
