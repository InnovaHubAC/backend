namespace Innova.Application.DTOs.Attachment
{
    public class AttachmentDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;
    }
}