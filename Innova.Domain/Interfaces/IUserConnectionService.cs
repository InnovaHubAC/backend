namespace Innova.Domain.Interfaces;

// this for tracking user connections in real-time features like chat
public interface IUserConnectionService
{
    Task AddConnectionAsync(string userId, string connectionId);
    Task RemoveConnectionAsync(string connectionId);
    Task<IEnumerable<string>> GetConnectionsAsync(string userId);
    Task<bool> IsUserOnlineAsync(string userId);
}
