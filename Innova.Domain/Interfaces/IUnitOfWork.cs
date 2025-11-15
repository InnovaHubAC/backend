namespace Innova.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDepartmentRepository DepartmentRepository { get; }
    IIdeaRepository IdeaRepository { get; }
    Task<int> CompleteAsync();
}