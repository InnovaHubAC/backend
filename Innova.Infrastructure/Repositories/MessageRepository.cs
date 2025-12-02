namespace Innova.Infrastructure.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    private readonly ApplicationDbContext _dbContext;

    public MessageRepository(ApplicationDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<IReadOnlyList<Message>> GetMessagesByConversationIdAsync(int conversationId, int pageSize = 50, int page = 1)
    {
        var skip = (page - 1) * pageSize;
        
        return await _dbContext.Set<Message>()
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUnreadMessageCountAsync(string userId)
    {
        return await _dbContext.Set<Message>()
            .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
    }

    public async Task MarkMessagesAsReadAsync(int conversationId, string userId)
    {
        var unreadMessages = await _dbContext.Set<Message>()
            .Where(m => m.ConversationId == conversationId && 
                        m.ReceiverId == userId && 
                        !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }
    }
}
