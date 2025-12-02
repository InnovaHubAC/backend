using Innova.Application.DTOs.Messaging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Innova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessagingService _messagingService;

    public MessagesController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    [HttpPost("send")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<MessageDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResponse<MessageDto>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ApiResponse<MessageDto>), (int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<ApiResponse<MessageDto>>> SendMessage([FromBody] SendMessageDto sendMessageDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _messagingService.SendMessageAsync(userId, sendMessageDto);
        return result.StatusCode switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }


    [HttpGet("conversations")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationDto>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationDto>>), (int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ConversationDto>>>> GetConversations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _messagingService.GetUserConversationsAsync(userId);
        return Ok(result);
    }

    [HttpGet("conversations/{conversationId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ConversationDetailDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResponse<ConversationDetailDto>), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ConversationDetailDto>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ConversationDetailDto>), (int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<ApiResponse<ConversationDetailDto>>> GetConversation(
        int conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _messagingService.GetConversationAsync(conversationId, userId, page, pageSize);
        return result.StatusCode switch
        {
            200 => Ok(result),
            403 => StatusCode(403, result),
            404 => NotFound(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    [HttpPost("conversations/with/{otherUserId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ConversationDetailDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResponse<ConversationDetailDto>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ConversationDetailDto>), (int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<ApiResponse<ConversationDetailDto>>> GetOrCreateConversation(string otherUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _messagingService.GetOrCreateConversationAsync(userId, otherUserId);
        return result.StatusCode switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    // mark all messages in a conversation as read
    [HttpPut("conversations/{conversationId}/read")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<bool>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), (int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(int conversationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _messagingService.MarkMessagesAsReadAsync(conversationId, userId);
        return result.StatusCode switch
        {
            200 => Ok(result),
            403 => StatusCode(403, result),
            404 => NotFound(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    [HttpGet("unread-count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<int>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResponse<int>), (int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _messagingService.GetUnreadMessageCountAsync(userId);
        return Ok(result);
    }
}
