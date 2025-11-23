using Innova.Application.DTOs.Comment;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Innova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("comments/{parentId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<CommentDto>>>> GetReplies(int parentId)
    {
        var result = await _commentService.GetRepliesByCommentIdAsync(parentId);
        return Ok(result);
    }

    [HttpPost("comments/{parentId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CommentDto>>> ReplyToComment(int parentId, [FromBody] CreateCommentDto createCommentDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var result = await _commentService.ReplyToCommentAsync(parentId, createCommentDto, userId);
        return result.StatusCode switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    [HttpPut("comments/{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateComment(int id, [FromBody] UpdateCommentDto updateCommentDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var result = await _commentService.UpdateCommentAsync(id, updateCommentDto, userId);
        return result.StatusCode switch
        {
            200 => Ok(result),
            401 => Unauthorized(result),
            404 => NotFound(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    [HttpDelete("comments/{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteComment(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var isAdmin = User.IsInRole("Admin");

        var result = await _commentService.DeleteCommentAsync(id, userId, isAdmin);
        return result.StatusCode switch
        {
            200 => Ok(result),
            401 => Unauthorized(result),
            404 => NotFound(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }
}
