namespace Innova.Application.DTOs.Vote;

public class VoteStatsDto
{
    public int UpvoteCount { get; set; }
    public int DownvoteCount { get; set; }
    public int TotalVotes => UpvoteCount + DownvoteCount;
    public VoteType? UserVoteType { get; set; }

}
