namespace Innova.Domain.Interfaces;

public interface IMessageRepository : IGenericRepository<Message>
{
    Task<IReadOnlyList<Message>> GetMessagesByConversationIdAsync(int conversationId, int pageSize = 50, int page = 1);
    Task<int> GetUnreadMessageCountAsync(string userId);
    Task MarkMessagesAsReadAsync(int conversationId, string userId);
}
