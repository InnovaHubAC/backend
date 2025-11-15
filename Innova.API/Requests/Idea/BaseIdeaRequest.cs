using Innova.Domain.Enums;

namespace Innova.API.Requests.Idea
{
    public class BaseIdeaRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AppUserId { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public IdeaStatus IdeaStatus { get; set; }
        public bool IsAnonymous { get; set; } = false;
        public IFormFile[]? Attachments { get; set; }
    }
}
