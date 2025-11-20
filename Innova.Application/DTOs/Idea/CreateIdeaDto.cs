using Innova.Application.DTOs.Common;
using Innova.Domain.Enums;

namespace Innova.Application.DTOs.Idea
{
    public class CreateIdeaDto : BaseIdeaDto
    {
        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}