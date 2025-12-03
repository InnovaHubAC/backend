using Innova.Application.DTOs.Vote;

namespace Innova.Application.Services.Interfaces;

public interface IVoteService
{
    Task<ApiResponse<VoteDto>> CastVoteAsync(int ideaId, CreateVoteDto createVoteDto, string userId);
    Task<ApiResponse<bool>> WithdrawVoteAsync(int ideaId, string userId);
}
