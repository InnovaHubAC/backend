namespace Innova.Application.DTOs.Idea
{
    public class UpdateIdeaDto : BaseIdeaDto
    {
        public int Id { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
        public List<int> RemovedAttachmentIds { get; set; } = new();
    }
}