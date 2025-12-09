namespace Innova.Infrastructure.Repositories;

public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ConversationRepository(ApplicationDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<Conversation?> GetConversationBetweenUsersAsync(string userOneId, string userTwoId)
    {
        return await _dbContext.Set<Conversation>()
            .FirstOrDefaultAsync(c => 
                (c.ParticipantOneId == userOneId && c.ParticipantTwoId == userTwoId) ||
                (c.ParticipantOneId == userTwoId && c.ParticipantTwoId == userOneId));
    }

    public async Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(string userId)
    {
        return await _dbContext.Set<Conversation>()
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(c => c.ParticipantOneId == userId || c.ParticipantTwoId == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Conversation?> GetConversationWithMessagesAsync(int conversationId, int pageSize = 10, int page = 1)
    {
        var conversation = await _dbContext.Set<Conversation>()
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
            return null;

        // Load messages with pagination
        var skip = (page - 1) * pageSize;
        conversation.Messages = await _dbContext.Set<Message>()
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return conversation;
    }
}
