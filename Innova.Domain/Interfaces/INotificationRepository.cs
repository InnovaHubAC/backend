namespace Innova.Domain.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId, int page, int pageSize);
    Task<int> GetUnreadCountAsync(string userId);
    Task<Notification?> GetByIdForUserAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
}
