namespace Innova.Domain.Entities;

public class Vote : BaseEntity
{
    public int IdeaId { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    public VoteType VoteType { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? WithdrawnAt { get; set; }
    public Idea Idea { get; set; } = null!;
}
