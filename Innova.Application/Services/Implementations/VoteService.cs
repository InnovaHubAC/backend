namespace Innova.Application.Services.Implementations;

public class VoteService : IVoteService
{
    private readonly IUnitOfWork _unitOfWork;

    public VoteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<VoteDto>> CastVoteAsync(int ideaId, CreateVoteDto createVoteDto, string userId)
    {
        // Validate that the idea exists
        var idea = await _unitOfWork.IdeaRepository.GetByIdAsync(ideaId);
        if (idea == null)
        {
            return ApiResponse<VoteDto>.Fail(404, "Idea not found");
        }

        // Check if user has already voted
        var existingVote = await _unitOfWork.VoteRepository.GetUserVoteForIdeaAsync(ideaId, userId);

        if (existingVote != null)
        {
            // User is changing their vote
            if (existingVote.VoteType == createVoteDto.VoteType)
            {
                return ApiResponse<VoteDto>.Fail(400, "You have already cast this vote");
            }

            existingVote.VoteType = createVoteDto.VoteType;
            existingVote.WithdrawnAt = null;
            _unitOfWork.VoteRepository.Update(existingVote);
            await _unitOfWork.CompleteAsync();
            return ApiResponse<VoteDto>.Success(existingVote.Adapt<VoteDto>());
        }

        // Create new vote
        var vote = new Vote
        {
            IdeaId = ideaId,
            AppUserId = userId,
            VoteType = createVoteDto.VoteType,
        };

        await _unitOfWork.VoteRepository.AddAsync(vote);
        await _unitOfWork.CompleteAsync();
        return ApiResponse<VoteDto>.Success(vote.Adapt<VoteDto>());
    }

    public async Task<ApiResponse<bool>> WithdrawVoteAsync(int ideaId, string userId)
    {
        var existingVote = await _unitOfWork.VoteRepository.GetUserVoteForIdeaAsync(ideaId, userId);

        if (existingVote == null)
        {
            return ApiResponse<bool>.Fail(404, "Vote not found");
        }

        existingVote.VoteType = VoteType.Withdraw;
        existingVote.WithdrawnAt = DateTime.UtcNow;
        _unitOfWork.VoteRepository.Update(existingVote);
        await _unitOfWork.CompleteAsync();
        return ApiResponse<bool>.Success(true);
    }
}