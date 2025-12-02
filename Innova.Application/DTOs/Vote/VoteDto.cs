namespace Innova.Application.DTOs.Vote;

public class VoteDto
{
    public int Id { get; set; }
    public int IdeaId { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    public VoteType VoteType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? WithdrawnAt { get; set; }
}
