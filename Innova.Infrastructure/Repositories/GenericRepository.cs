using System.Linq.Expressions;

namespace Innova.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T?> GetByIdWithIncludesAsync(int id, List<Expression<Func<T, object>>>? includeProperties)
    {
        IQueryable<T> query = _context.Set<T>();

        if (includeProperties is not null && includeProperties.Any())
            includeProperties.ForEach(include => query = query.Include(include));

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IReadOnlyList<T>> ListAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null)
    {
        IQueryable<T> query = _context.Set<T>();

        if (predicate != null)
            query = query.Where(predicate);

        if (includes != null)
            includes.ForEach(include => query = query.Include(include));

        if (orderBy != null)
            query = orderBy(query);

        return await query.ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>>? predicate = null,
        List<Expression<Func<T, object>>>? includes = null)
    {
        IQueryable<T> query = _context.Set<T>();

        if (includes != null)
            includes.ForEach(include => query = query.Include(include));

        if (predicate != null)
            return await query.FirstOrDefaultAsync(predicate);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate != null)
            return await _context.Set<T>().CountAsync(predicate);

        return await _context.Set<T>().CountAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().AnyAsync(predicate);
    }

    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }

    public void Update(T entity)
    {
        _context.Set<T>().Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public async Task DeleteAsync(T entity)
    {
        await _context.Set<T>().Where(x => x.Id == entity.Id).ExecuteDeleteAsync();
    }
}
