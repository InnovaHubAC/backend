namespace Innova.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Lazy<IDepartmentRepository> _departmentRepository;

    public IDepartmentRepository DepartmentRepository => _departmentRepository.Value;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _departmentRepository = new Lazy<IDepartmentRepository>(() => new DepartmentRepository(_context));
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
