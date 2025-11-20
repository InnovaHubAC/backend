using Innova.Application.DTOs.Common;
using Innova.Application.DTOs.Idea;

namespace Innova.Application.Validations.Idea
{
    public class UpdateIdeaDto : BaseIdeaDto
    {
        public int Id { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
        public List<int> RemovedAttachmentIds { get; set; } = new();
    }
}