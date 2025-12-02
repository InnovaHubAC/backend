using Innova.Application.DTOs.Vote;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Innova.API.Controllers;

[ApiController]
[Route("api/ideas/{ideaId}/[controller]")]
public class VotesController : ControllerBase
{
    private readonly IVoteService _voteService;

    public VotesController(IVoteService voteService)
    {
        _voteService = voteService;
    }

    /// <summary>
    /// Cast a vote (upvote or downvote) on an idea
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<VoteDto>>> CastVote(int ideaId, [FromBody] CreateVoteDto createVoteDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var result = await _voteService.CastVoteAsync(ideaId, createVoteDto, userId);
        return result.StatusCode switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            404 => NotFound(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    /// <summary>
    /// Withdraw a vote on an idea
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> WithdrawVote(int ideaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var result = await _voteService.WithdrawVoteAsync(ideaId, userId);
        return result.StatusCode switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }
}
