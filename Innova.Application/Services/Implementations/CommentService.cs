namespace Innova.Application.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;

    public CommentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IEnumerable<CommentDto>>> GetCommentsByIdeaIdAsync(int ideaId)
    {
        var spec = new CommentSpecifications(ideaId, CommentSpecType.ByIdea);
        var comments = await _unitOfWork.CommentRepository.ListAsync(spec);
        return ApiResponse<IEnumerable<CommentDto>>.Success(comments.Adapt<IEnumerable<CommentDto>>());
    }

    // TODO: this must be paginated if there are many replies
    public async Task<ApiResponse<IEnumerable<CommentDto>>> GetRepliesByCommentIdAsync(int commentId)
    {
        var spec = new CommentSpecifications(commentId, CommentSpecType.ByParent);
        var comments = await _unitOfWork.CommentRepository.ListAsync(spec);
        return ApiResponse<IEnumerable<CommentDto>>.Success(comments.Adapt<IEnumerable<CommentDto>>());
    }

    public async Task<ApiResponse<CommentDto>> CreateCommentAsync(int ideaId, CreateCommentDto createCommentDto, string userId)
    {
        var comment = createCommentDto.Adapt<Comment>();
        comment.IdeaId = ideaId;
        comment.AppUserId = userId;
        
        await _unitOfWork.CommentRepository.AddAsync(comment);
        await _unitOfWork.CompleteAsync();
        
        return ApiResponse<CommentDto>.Success(comment.Adapt<CommentDto>());
    }

    public async Task<ApiResponse<CommentDto>> ReplyToCommentAsync(int parentId, CreateCommentDto createCommentDto, string userId)
    {
        var parentComment = await _unitOfWork.CommentRepository.GetByIdAsync(parentId);
        if (parentComment == null)
        {
            return ApiResponse<CommentDto>.Fail(404, "Parent comment not found");
        }

        var comment = createCommentDto.Adapt<Comment>();
        comment.IdeaId = parentComment.IdeaId;
        comment.ParentId = parentId;
        comment.AppUserId = userId;

        await _unitOfWork.CommentRepository.AddAsync(comment);
        await _unitOfWork.CompleteAsync();

        return ApiResponse<CommentDto>.Success(comment.Adapt<CommentDto>());
    }

    public async Task<ApiResponse<bool>> UpdateCommentAsync(int id, UpdateCommentDto updateCommentDto, string userId)
    {
        var comment = await _unitOfWork.CommentRepository.GetByIdAsync(id);
        if (comment == null) return ApiResponse<bool>.Fail(404, "Comment not found");

        if (comment.AppUserId != userId) return ApiResponse<bool>.Fail(401, "Unauthorized");

        comment.Content = updateCommentDto.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        
        _unitOfWork.CommentRepository.Update(comment);
        await _unitOfWork.CompleteAsync();
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> DeleteCommentAsync(int id, string userId, bool isAdmin)
    {
        var comment = await _unitOfWork.CommentRepository.GetByIdAsync(id);
        if (comment == null) return ApiResponse<bool>.Fail(404, "Comment not found");

        if (comment.AppUserId != userId && !isAdmin) return ApiResponse<bool>.Fail(401, "Unauthorized");

        await _unitOfWork.CommentRepository.DeleteAsync(comment);
        await _unitOfWork.CompleteAsync();
        return ApiResponse<bool>.Success(true);
    }
}
