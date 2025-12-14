using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Innova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.GetRecentNotificationsAsync(userId, page, pageSize);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.GetUnreadCountAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{notificationId:int}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(int notificationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.MarkAsReadAsync(notificationId, userId);
        return result.StatusCode switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _notificationService.MarkAllAsReadAsync(userId);
        return StatusCode(result.StatusCode, result);
    }
}
