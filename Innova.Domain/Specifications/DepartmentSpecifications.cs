namespace Innova.Domain.Specifications;

/// <summary>
/// Specification for getting all departments
/// </summary>
public class GetAllDepartmentsSpecification : BaseSpecification<Department>
{
    public GetAllDepartmentsSpecification() : base()
    {
        ApplyOrderBy(d => d.Name);
    }
}

/// <summary>
/// Specification for getting a department by ID
/// </summary>
public class GetDepartmentByIdSpecification : BaseSpecification<Department>
{
    public GetDepartmentByIdSpecification(int id) : base(d => d.Id == id)
    {
    }
}

/// <summary>
/// Specification for getting a department by name
/// </summary>
public class GetDepartmentByNameSpecification : BaseSpecification<Department>
{
    public GetDepartmentByNameSpecification(string name) 
        : base(d => d.Name.ToLower() == name.ToLower())
    {
    }
}
