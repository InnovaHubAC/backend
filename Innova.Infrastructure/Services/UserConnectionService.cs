using System.Collections.Concurrent;

namespace Innova.Infrastructure.Services;

public class UserConnectionService : IUserConnectionService
{
    // In-memory storage for user connections
    // TODO: We will use Redis later
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    private static readonly ConcurrentDictionary<string, string> _connectionUserMap = new();

    public Task AddConnectionAsync(string userId, string connectionId)
    {
        _userConnections.AddOrUpdate(
            userId,
            new HashSet<string> { connectionId },
            (_, connections) =>
            {
                connections.Add(connectionId);
                return connections;
            });

        _connectionUserMap.TryAdd(connectionId, userId);

        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(string connectionId)
    {
        if (_connectionUserMap.TryRemove(connectionId, out var userId))
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetConnectionsAsync(string userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            return Task.FromResult<IEnumerable<string>>(connections.ToList());
        }

        return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
    }

    public Task<bool> IsUserOnlineAsync(string userId)
    {
        var isOnline = _userConnections.TryGetValue(userId, out var connections) && connections.Count > 0;
        return Task.FromResult(isOnline);
    }
}
