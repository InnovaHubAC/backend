namespace Innova.Application.DTOs.Idea
{
    public class CreateIdeaDto : BaseIdeaDto
    {
        public List<FileAttachmentDto> Attachments { get; set; } = new();
    }
}