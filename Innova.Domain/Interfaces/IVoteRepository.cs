namespace Innova.Domain.Interfaces;

public interface IVoteRepository : IGenericRepository<Vote>
{
    Task<Vote?> GetUserVoteForIdeaAsync(int ideaId, string userId);
    Task<int> GetUpvoteCountForIdeaAsync(int ideaId);
    Task<int> GetDownvoteCountForIdeaAsync(int ideaId);
}
