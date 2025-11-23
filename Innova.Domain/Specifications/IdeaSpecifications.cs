namespace Innova.Domain.Specifications;

public class GetIdeasByUserIdSpecification : BaseSpecification<Idea>
{
    public GetIdeasByUserIdSpecification(string userId) : base(i => i.AppUserId == userId)
    {
        AddInclude(i => i.Department);
        AddInclude(i => i.Attachments!);
        ApplyOrderByDescending(i => i.CreatedAt);
    }
}
