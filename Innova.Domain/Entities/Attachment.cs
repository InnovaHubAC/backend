namespace Innova.Domain.Entities
{
    public class Attachment : BaseEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;
        public int IdeaId { get; set; }
        public Idea Idea { get; set; } = null!;
    }
}
