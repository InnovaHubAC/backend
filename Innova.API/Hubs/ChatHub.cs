using Innova.Application.DTOs.Messaging;
using Innova.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Innova.API.Hubs;

public class ChatHub : Hub
{
    private readonly IUserConnectionService _userConnectionService;
    private readonly IMessagingService _messagingService;

    public ChatHub(
        IUserConnectionService userConnectionService,
        IMessagingService messagingService)
    {
        _userConnectionService = userConnectionService;
        _messagingService = messagingService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await _userConnectionService.AddConnectionAsync(userId, Context.ConnectionId);
            
            // Notify other users that this user is online
            await Clients.Others.SendAsync("UserStatusChanged", new { UserId = userId, IsOnline = true });
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _userConnectionService.RemoveConnectionAsync(Context.ConnectionId);

        if (!string.IsNullOrEmpty(userId))
        {
            var isStillOnline = await _userConnectionService.IsUserOnlineAsync(userId);
            
            if (!isStillOnline)
            {
                // Notify other users that this user went offline
                await Clients.Others.SendAsync("UserStatusChanged", new { UserId = userId, IsOnline = false });
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string receiverId, string content)
    {
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(senderId))
        {
            await Clients.Caller.SendAsync("Error", "You must be authenticated to send messages");
            return;
        }

        var result = await _messagingService.SendMessageAsync(senderId, new SendMessageDto
        {
            ReceiverId = receiverId,
            Content = content
        });

        if (result.StatusCode == 200 && result.Data != null)
        {
            // Send confirmation back to sender
            await Clients.Caller.SendAsync("MessageSent", result.Data);
            
            // Send message to receiver if they're online
            var receiverConnections = await _userConnectionService.GetConnectionsAsync(receiverId);
            foreach (var connectionId in receiverConnections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", result.Data);
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Error", result.Message);
        }
    }

    public async Task MarkAsRead(int conversationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("Error", "You must be authenticated");
            return;
        }

        var result = await _messagingService.MarkMessagesAsReadAsync(conversationId, userId);
        
        if (result.StatusCode == 200)
        {
            var conversationResult = await _messagingService.GetConversationAsync(conversationId, userId);
            if (conversationResult.StatusCode == 200 && conversationResult.Data != null)
            {
                var otherUserId = conversationResult.Data.OtherParticipantId;
                
                var otherUserConnections = await _userConnectionService.GetConnectionsAsync(otherUserId);
                foreach (var connectionId in otherUserConnections)
                {
                    await Clients.Client(connectionId).SendAsync("MessagesRead", conversationId);
                }
            }
        }
    }

    public async Task<bool> IsUserOnline(string userId)
    {
        return await _userConnectionService.IsUserOnlineAsync(userId);
    }

    public async Task StartTyping(int conversationId, string receiverId)
    {
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(senderId))
            return;

        var receiverConnections = await _userConnectionService.GetConnectionsAsync(receiverId);
        
        foreach (var connectionId in receiverConnections)
        {
            await Clients.Client(connectionId).SendAsync("UserTyping", new 
            { 
                ConversationId = conversationId, 
                UserId = senderId 
            });
        }
    }

    public async Task StopTyping(int conversationId, string receiverId)
    {
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(senderId))
            return;

        var receiverConnections = await _userConnectionService.GetConnectionsAsync(receiverId);
        
        foreach (var connectionId in receiverConnections)
        {
            await Clients.Client(connectionId).SendAsync("UserStoppedTyping", new 
            { 
                ConversationId = conversationId, 
                UserId = senderId 
            });
        }
    }
}
