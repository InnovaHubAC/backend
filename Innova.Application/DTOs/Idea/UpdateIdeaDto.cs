namespace Innova.Application.DTOs.Idea
{
    public class UpdateIdeaDto : BaseIdeaDto
    {
        public int Id { get; set; }
        public List<FileAttachmentDto> Attachments { get; set; } = new();
        public List<int> RemovedAttachmentIds { get; set; } = new();
    }
}