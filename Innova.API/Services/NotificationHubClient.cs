using Innova.API.Hubs;
using Innova.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Innova.API.Services;

public class NotificationHubClient : INotificationHubClient
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IUserConnectionService _userConnectionService;
    public NotificationHubClient(IHubContext<NotificationHub> hubContext,
        IUserConnectionService userConnectionService)
    {
        _hubContext = hubContext;
        _userConnectionService = userConnectionService;
    }

    public async Task DispatchAsync(string recipientId, NotificationDto notification)
    {
        var connections = await _userConnectionService.GetConnectionsAsync(recipientId);

        foreach (var connectionId in connections)
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("NotificationReceived", notification);
    }
}
