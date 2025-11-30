namespace Innova.Domain.Interfaces;

public interface IConversationRepository : IGenericRepository<Conversation>
{
    Task<Conversation?> GetConversationBetweenUsersAsync(string userOneId, string userTwoId);
    Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(string userId);
    Task<Conversation?> GetConversationWithMessagesAsync(int conversationId, int pageSize = 50, int page = 1);
}
