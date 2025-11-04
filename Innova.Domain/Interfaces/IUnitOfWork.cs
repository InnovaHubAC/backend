namespace Innova.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> CompleteAsync();
}