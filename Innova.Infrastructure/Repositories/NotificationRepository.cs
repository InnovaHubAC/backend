namespace Innova.Infrastructure.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdForUserAsync(int notificationId, string userId)
    {
        return await _context.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);
    }

    public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId, int page, int pageSize)
    {
        return await _context.Set<Notification>()
            .Where(n => n.RecipientId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Set<Notification>()
            .CountAsync(n => n.RecipientId == userId && !n.IsRead);
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _context.Set<Notification>()
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, _ => DateTime.UtcNow));
    }
}
