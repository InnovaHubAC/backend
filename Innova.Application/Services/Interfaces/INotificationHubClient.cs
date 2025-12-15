namespace Innova.Application.Services.Interfaces;


/// <summary>
/// Defines a contract for sending real-time notifications, where the implementation is expected to use SignalR hubs
/// for dispatching notifications to connected clients, where the implementation will be in the API layer
/// since it can deal with SignalR (as well as HTTP).
/// I need to do this since the services that need to send real-time notifications are in the Application layer,
/// where they need to call some endpoints in the hub and the application layer should not depend on the API layer.
/// </summary>
public interface INotificationHubClient
{
    Task DispatchAsync(string recipientId, NotificationDto notification);
}
