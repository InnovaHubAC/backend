using System.Text.Json;

namespace Innova.Application.Services.Implementations;

public class NotificationService : INotificationService
{
    private const int DefaultMaxPageSize = 50;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly INotificationHubClient _notificationHubClient;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        INotificationHubClient notificationHubClient)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _notificationHubClient = notificationHubClient;
    }

    public async Task PublishIdeaVoteNotificationAsync(Idea idea, Vote vote,
        string triggeredByUserId)
    {
        if (idea.AppUserId == triggeredByUserId)
        {
            return;
        }

        var recipientId = idea.AppUserId;
        var voter = await _identityService.GetUserByIdAsync(triggeredByUserId);
        var voterDisplayName = "Someone";

        if (voter.HasValue)
        {
            var firstName = voter.Value.FirstName;
            var lastName = voter.Value.LastName;
            var nameParts = new List<string>();

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                nameParts.Add(firstName.Trim());
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                nameParts.Add(lastName.Trim());
            }

            if (nameParts.Count > 0)
            {
                voterDisplayName = string.Join(' ', nameParts);
            }
            else if (!string.IsNullOrWhiteSpace(voter.Value.UserName))
            {
                voterDisplayName = voter.Value.UserName!;
            }
        }

        var voteAction = vote.VoteType switch
        {
            VoteType.Upvote => "upvoted",
            VoteType.Downvote => "downvoted",
            _ => "updated their vote on"
        };

        var notification = new Notification
        {
            RecipientId = recipientId,
            TriggeredById = triggeredByUserId,
            NotificationType = NotificationType.IdeaVote,
            Title = "Your idea received a vote",
            Message = $"{voterDisplayName} {voteAction} your idea '{idea.Title}'.",
            IdeaId = idea.Id,
            VoteId = vote.Id,
        };

        await _unitOfWork.NotificationRepository.AddAsync(notification);
        await _unitOfWork.CompleteAsync();

        var notificationDto = notification.Adapt<NotificationDto>();
        await _notificationHubClient.DispatchAsync(recipientId, notificationDto);
    }

    public async Task<ApiResponse<IEnumerable<NotificationDto>>> GetRecentNotificationsAsync(
        string userId,
        int page = 1,
        int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, DefaultMaxPageSize);

        var notifications = await _unitOfWork.NotificationRepository
            .GetUserNotificationsAsync(userId, page, pageSize);

        var mapped = notifications.Adapt<IEnumerable<NotificationDto>>();
        return ApiResponse<IEnumerable<NotificationDto>>.Success(mapped);
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(string userId)
    {
        var count = await _unitOfWork.NotificationRepository.GetUnreadCountAsync(userId);
        return ApiResponse<int>.Success(count);
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _unitOfWork.NotificationRepository.
            GetByIdForUserAsync(notificationId, userId);
        if (notification is null)
        {
            return ApiResponse<bool>.Fail(404, "Notification not found");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            _unitOfWork.NotificationRepository.Update(notification);
            await _unitOfWork.CompleteAsync();
        }

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(string userId)
    {
        await _unitOfWork.NotificationRepository.MarkAllAsReadAsync(userId);
        await _unitOfWork.CompleteAsync();
        return ApiResponse<bool>.Success(true);
    }
}
