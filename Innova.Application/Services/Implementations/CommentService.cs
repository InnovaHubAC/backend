namespace Innova.Application.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CommentService> _logger;
    private readonly ICacheService _cacheService;

    public CommentService(
        IUnitOfWork unitOfWork, 
        ILogger<CommentService> logger,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<IEnumerable<CommentDto>>> GetCommentsByIdeaIdAsync(int ideaId)
    {
        var cacheKey = $"Comment:ByIdeaId:{ideaId}";

        var cachedResult = _cacheService.Get<ApiResponse<IEnumerable<CommentDto>>>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation(
                "Cache hit for comments by idea. CacheKey: {CacheKey}, IdeaId: {IdeaId}",
                cacheKey,
                ideaId);
            return cachedResult;
        }

        var comments = await _unitOfWork.CommentRepository.ListAsync(
            predicate: c => c.IdeaId == ideaId && c.ParentId == null,
            orderBy: q => q.OrderByDescending(c => c.CreatedAt));

        var commentDtos = comments.Adapt<IEnumerable<CommentDto>>();
        var response = ApiResponse<IEnumerable<CommentDto>>.Success(commentDtos);

        _cacheService.Set(
            cacheKey, 
            response, 
            _cacheService.SetMemoryCacheEntryOptions(
                absoluteExpiration: TimeSpan.FromMinutes(10),
                slidingExpiration: TimeSpan.FromMinutes(3)));

        _logger.LogInformation(
            "Comments retrieved successfully for idea. IdeaId: {IdeaId}, CommentCount: {CommentCount}",
            ideaId,
            comments.Count());

        return response;
    }

    // TODO: this must be paginated if there are many replies
    public async Task<ApiResponse<IEnumerable<CommentDto>>> GetRepliesByCommentIdAsync(int commentId)
    {
        var cacheKey = $"Comment:Replies:{commentId}";

        var cachedResult = _cacheService.Get<ApiResponse<IEnumerable<CommentDto>>>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation(
                "Cache hit for comment replies. CacheKey: {CacheKey}, CommentId: {CommentId}",
                cacheKey,
                commentId);
            return cachedResult;
        }

        var comments = await _unitOfWork.CommentRepository.ListAsync(
            predicate: c => c.ParentId == commentId,
            orderBy: q => q.OrderBy(c => c.CreatedAt));

        var commentDtos = comments.Adapt<IEnumerable<CommentDto>>();
        var response = ApiResponse<IEnumerable<CommentDto>>.Success(commentDtos);

        // Cache the result for 5 minutes
        _cacheService.Set(
            cacheKey, 
            response, 
            _cacheService.SetMemoryCacheEntryOptions(
                absoluteExpiration: TimeSpan.FromMinutes(5),
                slidingExpiration: TimeSpan.FromMinutes(2)));

        _logger.LogInformation(
            "Replies retrieved successfully for comment. CommentId: {CommentId}, ReplyCount: {ReplyCount}",
            commentId,
            comments.Count());

        return response;
    }

    public async Task<ApiResponse<CommentDto>> CreateCommentAsync(int ideaId, CreateCommentDto createCommentDto, string userId)
    {
        var comment = createCommentDto.Adapt<Comment>();
        comment.IdeaId = ideaId;
        comment.AppUserId = userId;
        
        await _unitOfWork.CommentRepository.AddAsync(comment);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Comment created successfully. CommentId: {CommentId}, IdeaId: {IdeaId}, UserId: {UserId}",
            comment.Id,
            ideaId,
            userId);

        InvalidateCommentCacheForIdea(ideaId);
        
        return ApiResponse<CommentDto>.Success(comment.Adapt<CommentDto>());
    }

    public async Task<ApiResponse<CommentDto>> ReplyToCommentAsync(int parentId, CreateCommentDto createCommentDto, string userId)
    {
        var parentComment = await _unitOfWork.CommentRepository.GetByIdAsync(parentId);
        if (parentComment == null)
        {
            _logger.LogWarning(
                "Reply failed. Parent comment not found. ParentCommentId: {ParentCommentId}, UserId: {UserId}",
                parentId,
                userId);
            return ApiResponse<CommentDto>.Fail(404, "Parent comment not found");
        }

        var comment = createCommentDto.Adapt<Comment>();
        comment.IdeaId = parentComment.IdeaId;
        comment.ParentId = parentId;
        comment.AppUserId = userId;

        await _unitOfWork.CommentRepository.AddAsync(comment);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Reply created successfully. CommentId: {CommentId}, ParentCommentId: {ParentCommentId}, IdeaId: {IdeaId}, UserId: {UserId}",
            comment.Id,
            parentId,
            parentComment.IdeaId,
            userId);

        InvalidateRepliesCache(parentId);
        InvalidateCommentCacheForIdea(parentComment.IdeaId);

        return ApiResponse<CommentDto>.Success(comment.Adapt<CommentDto>());
    }

    public async Task<ApiResponse<bool>> UpdateCommentAsync(int id, UpdateCommentDto updateCommentDto, string userId)
    {
        var comment = await _unitOfWork.CommentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            _logger.LogWarning(
                "Update failed. Comment not found. CommentId: {CommentId}, UserId: {UserId}",
                id,
                userId);
            return ApiResponse<bool>.Fail(404, "Comment not found");
        }

        if (comment.AppUserId != userId)
        {
            _logger.LogWarning(
                "Update failed. Unauthorized access attempt. CommentId: {CommentId}, CommentOwnerId: {CommentOwnerId}, RequestingUserId: {RequestingUserId}",
                id,
                comment.AppUserId,
                userId);
            return ApiResponse<bool>.Fail(401, "Unauthorized");
        }

        var oldContent = comment.Content;
        comment.Content = updateCommentDto.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        
        _unitOfWork.CommentRepository.Update(comment);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Comment updated successfully. CommentId: {CommentId}, UserId: {UserId}, ContentChanged: {ContentChanged}",
            id,
            userId,
            oldContent != updateCommentDto.Content);

        InvalidateCommentCacheForIdea(comment.IdeaId);
        if (comment.ParentId.HasValue)
        {
            InvalidateRepliesCache(comment.ParentId.Value);
        }

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> DeleteCommentAsync(int id, string userId, bool isAdmin)
    {
        var comment = await _unitOfWork.CommentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            _logger.LogWarning(
                "Delete failed. Comment not found. CommentId: {CommentId}, UserId: {UserId}",
                id,
                userId);
            return ApiResponse<bool>.Fail(404, "Comment not found");
        }

        if (comment.AppUserId != userId && !isAdmin)
        {
            _logger.LogWarning(
                "Delete failed. Unauthorized access attempt. CommentId: {CommentId}, CommentOwnerId: {CommentOwnerId}, RequestingUserId: {RequestingUserId}, IsAdmin: {IsAdmin}",
                id,
                comment.AppUserId,
                userId,
                isAdmin);
            return ApiResponse<bool>.Fail(401, "Unauthorized");
        }

        var ideaId = comment.IdeaId;
        var parentId = comment.ParentId;

        await _unitOfWork.CommentRepository.DeleteAsync(comment);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Comment deleted successfully. CommentId: {CommentId}, IdeaId: {IdeaId}, DeletedBy: {UserId}, IsAdmin: {IsAdmin}",
            id,
            ideaId,
            userId,
            isAdmin);

        InvalidateCommentCacheForIdea(ideaId);
        if (parentId.HasValue)
        {
            InvalidateRepliesCache(parentId.Value);
        }

        return ApiResponse<bool>.Success(true);
    }

    private void InvalidateCommentCacheForIdea(int ideaId)
    {
        var cacheKey = $"Comment:ByIdeaId:{ideaId}";
        _cacheService.Remove(cacheKey);

        _logger.LogInformation(
            "Cache invalidated for idea comments. CacheKey: {CacheKey}, IdeaId: {IdeaId}, Reason: {Reason}",
            cacheKey,
            ideaId,
            "Comment data modified");
    }

    private void InvalidateRepliesCache(int parentCommentId)
    {
        var cacheKey = $"Comment:Replies:{parentCommentId}";
        _cacheService.Remove(cacheKey);

        _logger.LogInformation(
            "Cache invalidated for comment replies. CacheKey: {CacheKey}, ParentCommentId: {ParentCommentId}, Reason: {Reason}",
            cacheKey,
            parentCommentId,
            "Reply data modified");
    }
}
