namespace Innova.Infrastructure.Repositories
{
    public class IdeaRepository : GenericRepository<Idea>, IIdeaRepository
    {
        private readonly ApplicationDbContext _context;

        public IdeaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(IReadOnlyList<Idea> Ideas, int TotalCount)> GetAllIdeasPagedAsync(
            int pageIndex,
            int pageSize,
            string? sort)
        {
            var query = _context.Set<Idea>()
                .Include(i => i.Attachments)
                .Include(i => i.Department)
                .Include(i => i.Votes)
                .AsQueryable();

            query = sort?.ToLower() switch
            {
                "newest" => query.OrderByDescending(i => i.CreatedAt),
                "oldest" => query.OrderBy(i => i.CreatedAt),
                "title" => query.OrderBy(i => i.Title),
                "title_desc" => query.OrderByDescending(i => i.Title),
                "status" => query.OrderBy(i => i.IdeaStatus),
                "status_desc" => query.OrderByDescending(i => i.IdeaStatus),
                "upvotes" => query.OrderByDescending(i => i.Votes.Count(v => v.VoteType == VoteType.Upvote)),
                "upvotes_asc" => query.OrderBy(i => i.Votes.Count(v => v.VoteType == VoteType.Upvote)),
                _ => query.OrderByDescending(i => i.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var ideas = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (ideas, totalCount);
        }
    }
}