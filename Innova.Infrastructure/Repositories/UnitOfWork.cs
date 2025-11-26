namespace Innova.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Lazy<IDepartmentRepository> _departmentRepository;
    private readonly Lazy<IIdeaRepository> _ideaRepository;
    private readonly Lazy<IGenericRepository<Comment>> _commentRepository;

    public IDepartmentRepository DepartmentRepository => _departmentRepository.Value;
    public IIdeaRepository IdeaRepository => _ideaRepository.Value;
    public IGenericRepository<Comment> CommentRepository => _commentRepository.Value;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _departmentRepository = new Lazy<IDepartmentRepository>(() => new DepartmentRepository(_context));
        _ideaRepository = new Lazy<IIdeaRepository>(() => new IdeaRepository(_context));
        _commentRepository = new Lazy<IGenericRepository<Comment>>(() => new GenericRepository<Comment>(_context));
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
