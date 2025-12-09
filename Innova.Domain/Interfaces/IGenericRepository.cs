namespace Innova.Domain.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> ListAllAsync();
    Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null);

        Task<(IReadOnlyList<T> Items, int TotalCount)> ListPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>>? predicate = null,
        List<Expression<Func<T, object>>>? includes = null);
    
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    Task DeleteAsync(T entity);
    Task<T?> GetByIdWithIncludesAsync(int id, List<Expression<Func<T, object>>>? includeProperties);
}
