namespace Innova.Domain.Interfaces
{
    public interface IIdeaRepository : IGenericRepository<Idea>
    {
        Task<(IReadOnlyList<Idea> Ideas, int TotalCount)> GetAllIdeasPagedAsync(
            int pageIndex,
            int pageSize,
            string? sort);
    }
}