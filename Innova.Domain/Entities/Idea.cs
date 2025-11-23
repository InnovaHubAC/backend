namespace Innova.Domain.Entities
{
    public class Idea : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        // use string because IdentityUser's key is string
        public string AppUserId { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public IdeaStatus IdeaStatus { get; set; }
        public bool IsAnonymous { get; set; } = false;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Attachment>? Attachments { get; set; } = new List<Attachment>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public Department Department { get; set; } = null!;
    }
}
