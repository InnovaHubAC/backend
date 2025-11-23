using Innova.Application.DTOs.Attachment;
using Innova.Application.DTOs.Users;
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
        public UserDto User { get; set; } = null!;
    }
}
