using Innova.Domain.Entities;

namespace Innova.Domain.Specifications;

public enum CommentSpecType
{
    ByIdea,
    ByParent,
    ById
}

public class CommentSpecifications : BaseSpecification<Comment>
{
    public CommentSpecifications(int id, CommentSpecType type) 
        : base(c => 
            (type == CommentSpecType.ByIdea ? c.IdeaId == id && c.ParentId == null : true) &&
            (type == CommentSpecType.ByParent ? c.ParentId == id : true) &&
            (type == CommentSpecType.ById ? c.Id == id : true)
        )
    {
        if (type == CommentSpecType.ByIdea)
        {
            ApplyOrderByDescending(c => c.CreatedAt);
        }
        else if (type == CommentSpecType.ByParent)
        {
            ApplyOrderBy(c => c.CreatedAt);
        }
    }
}
