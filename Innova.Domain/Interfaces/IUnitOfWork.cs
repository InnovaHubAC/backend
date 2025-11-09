namespace Innova.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDepartmentRepository DepartmentRepository { get; }
    Task<int> CompleteAsync();
}