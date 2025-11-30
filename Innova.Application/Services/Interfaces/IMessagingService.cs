using Innova.Application.DTOs.Messaging;

namespace Innova.Application.Services.Interfaces;

public interface IMessagingService
{
    Task<ApiResponse<MessageDto>> SendMessageAsync(string senderId, SendMessageDto sendMessageDto);
    Task<ApiResponse<IEnumerable<ConversationDto>>> GetUserConversationsAsync(string userId);
    Task<ApiResponse<ConversationDetailDto>> GetConversationAsync(int conversationId, string userId, int page = 1, int pageSize = 50);
    Task<ApiResponse<ConversationDetailDto>> GetOrCreateConversationAsync(string currentUserId, string otherUserId);
    Task<ApiResponse<bool>> MarkMessagesAsReadAsync(int conversationId, string userId);
    Task<ApiResponse<int>> GetUnreadMessageCountAsync(string userId);
}
