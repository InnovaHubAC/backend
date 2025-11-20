namespace Innova.API.Requests.Idea
{
    public class UpdateIdeaRequest : BaseIdeaRequest
    {
        public int Id { get; set; }
        public List<int> RemovedAttachmentIds { get; set; } = new();
    }
}
