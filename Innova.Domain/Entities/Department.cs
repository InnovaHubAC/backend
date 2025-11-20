namespace Innova.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<Idea>? Ideas { get; set; } = new List<Idea>();
}
