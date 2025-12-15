namespace Innova.Application.Services.Implementations;

public class VoteService : IVoteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<VoteService> _logger;

    public VoteService(IUnitOfWork unitOfWork, ILogger<VoteService> logger,INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<VoteDto>> CastVoteAsync(int ideaId, CreateVoteDto createVoteDto, string userId)
    {
        // Validate that the idea exists
        var idea = await _unitOfWork.IdeaRepository.GetByIdAsync(ideaId);
        if (idea == null)
        {
            _logger.LogWarning(
                "Vote cast failed. Idea not found. IdeaId: {IdeaId}, UserId: {UserId}",
                ideaId,
                userId);
            return ApiResponse<VoteDto>.Fail(404, "Idea not found");
        }

        // Check if user has already voted
        var existingVote = await _unitOfWork.VoteRepository.GetUserVoteForIdeaAsync(ideaId, userId);

        if (existingVote != null)
        {
            // User is changing their vote
            if (existingVote.VoteType == createVoteDto.VoteType)
            {
                _logger.LogWarning(
                    "Vote cast failed. User has already cast this vote type. IdeaId: {IdeaId}, UserId: {UserId}, VoteType: {VoteType}",
                    ideaId,
                    userId,
                    createVoteDto.VoteType);
                return ApiResponse<VoteDto>.Fail(400, "You have already cast this vote");
            }

            var oldVoteType = existingVote.VoteType;
            existingVote.VoteType = createVoteDto.VoteType;
            existingVote.WithdrawnAt = null;
            _unitOfWork.VoteRepository.Update(existingVote);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Vote changed successfully. VoteId: {VoteId}, IdeaId: {IdeaId}, UserId: {UserId}, Changes: {@Changes}",
                existingVote.Id,
                ideaId,
                userId,
                new { OldVoteType = oldVoteType, NewVoteType = createVoteDto.VoteType });

            // TODO: may to make it as a background job
            await _notificationService.PublishIdeaVoteNotificationAsync(idea, existingVote, userId);
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
        // TODO: may to make it as a background job
        await _notificationService.PublishIdeaVoteNotificationAsync(idea, vote, userId);

        _logger.LogInformation(
            "Vote cast successfully. VoteId: {VoteId}, IdeaId: {IdeaId}, UserId: {UserId}, VoteType: {VoteType}",
            vote.Id,
            ideaId,
            userId,
            createVoteDto.VoteType);

        return ApiResponse<VoteDto>.Success(vote.Adapt<VoteDto>());
    }

    public async Task<ApiResponse<bool>> WithdrawVoteAsync(int ideaId, string userId)
    {
        var existingVote = await _unitOfWork.VoteRepository.GetUserVoteForIdeaAsync(ideaId, userId);

        if (existingVote == null)
        {
            _logger.LogWarning(
                "Vote withdrawal failed. Vote not found. IdeaId: {IdeaId}, UserId: {UserId}",
                ideaId,
                userId);
            return ApiResponse<bool>.Fail(404, "Vote not found");
        }

        var previousVoteType = existingVote.VoteType;
        existingVote.VoteType = VoteType.Withdraw;
        existingVote.WithdrawnAt = DateTime.UtcNow;
        _unitOfWork.VoteRepository.Update(existingVote);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Vote withdrawn successfully. VoteId: {VoteId}, IdeaId: {IdeaId}, UserId: {UserId}, PreviousVoteType: {PreviousVoteType}",
            existingVote.Id,
            ideaId,
            userId,
            previousVoteType);

        return ApiResponse<bool>.Success(true);
    }
}