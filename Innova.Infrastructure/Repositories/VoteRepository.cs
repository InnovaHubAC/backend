namespace Innova.Infrastructure.Repositories;

public class VoteRepository : GenericRepository<Vote>, IVoteRepository
{
    private readonly ApplicationDbContext _context;

    public VoteRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Vote?> GetUserVoteForIdeaAsync(int ideaId, string userId)
    {
        return await _context.Set<Vote>()
            .FirstOrDefaultAsync(v => v.IdeaId == ideaId && v.AppUserId == userId);
    }

    public async Task<int> GetUpvoteCountForIdeaAsync(int ideaId)
    {
        return await _context.Set<Vote>()
            .CountAsync(v => v.IdeaId == ideaId && v.VoteType == VoteType.Upvote);
    }

    public async Task<int> GetDownvoteCountForIdeaAsync(int ideaId)
    {
        return await _context.Set<Vote>()
            .CountAsync(v => v.IdeaId == ideaId && v.VoteType == VoteType.Downvote);
    }
}
