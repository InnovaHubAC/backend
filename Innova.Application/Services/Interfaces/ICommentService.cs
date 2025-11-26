using Innova.Application.DTOs;
using Innova.Application.DTOs.Comment;

namespace Innova.Application.Services.Interfaces;

public interface ICommentService
{
    Task<ApiResponse<IEnumerable<CommentDto>>> GetCommentsByIdeaIdAsync(int ideaId);
    Task<ApiResponse<IEnumerable<CommentDto>>> GetRepliesByCommentIdAsync(int commentId);
    Task<ApiResponse<CommentDto>> CreateCommentAsync(int ideaId, CreateCommentDto createCommentDto, string userId);
    Task<ApiResponse<CommentDto>> ReplyToCommentAsync(int parentId, CreateCommentDto createCommentDto, string userId);
    Task<ApiResponse<bool>> UpdateCommentAsync(int id, UpdateCommentDto updateCommentDto, string userId);
    Task<ApiResponse<bool>> DeleteCommentAsync(int id, string userId, bool isAdmin);
}
