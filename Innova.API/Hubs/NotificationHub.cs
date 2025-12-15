using System.Security.Claims;
using Innova.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Innova.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly IUserConnectionService _userConnectionService;
    public NotificationHub(IUserConnectionService userConnectionService)
    {
        _userConnectionService = userConnectionService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await _userConnectionService.AddConnectionAsync(userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _userConnectionService.RemoveConnectionAsync(Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}
