namespace Innova.Infrastructure.Repositories
{
    public class IdeaRepository : GenericRepository<Idea>, IIdeaRepository
    {
        private readonly ApplicationDbContext _context;

        public IdeaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}