namespace Innova.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDepartmentRepository DepartmentRepository { get; }
    IIdeaRepository IdeaRepository { get; }
    IGenericRepository<Comment> CommentRepository { get; }
    Task<int> CompleteAsync();
}