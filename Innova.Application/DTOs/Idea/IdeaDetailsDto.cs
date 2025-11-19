using Innova.Application.DTOs.Attachment;
using Innova.Domain.Enums;

namespace Innova.Application.DTOs.Idea
{
    public class IdeaDetailsDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public IdeaStatus IdeaStatus { get; set; }
        public bool IsAnonymous { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<AttachmentDto> IdeaAttachments { get; set; } = null!;
        public DepartmentDto Department { get; set; } = null!;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
