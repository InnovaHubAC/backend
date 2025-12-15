namespace Innova.Application.Services.Interfaces;

public interface INotificationService
{
    Task PublishIdeaVoteNotificationAsync(Idea idea, Vote vote, string triggeredByUserId);
    Task PublishIdeaCommentNotificationAsync(Idea idea, Comment comment, string triggeredByUserId);
    Task<ApiResponse<IEnumerable<NotificationDto>>> GetRecentNotificationsAsync(string userId, int page = 1, int pageSize = 20);
    Task<ApiResponse<int>> GetUnreadCountAsync(string userId);
    Task<ApiResponse<bool>> MarkAsReadAsync(int notificationId, string userId);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(string userId);
}
