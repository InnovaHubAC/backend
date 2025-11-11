namespace Innova.Application.Services.Implementations;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _departmentRepository = unitOfWork.DepartmentRepository;
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetAllDepartmentsAsync()
    {
        var spec = new GetAllDepartmentsSpecification();
        var departments = await _departmentRepository.ListAsync(spec);
        
        return departments.Adapt<IReadOnlyList<DepartmentDto>>();
    }

    public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
    {
        var spec = new GetDepartmentByIdSpecification(id);
        var department = await _departmentRepository.GetEntityWithSpec(spec);
        
        return department?.Adapt<DepartmentDto>();
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto createDto)
    {
        var existingSpec = new GetDepartmentByNameSpecification(createDto.Name);
        var existing = await _departmentRepository.GetEntityWithSpec(existingSpec);
        
        if (existing != null)
        {
            throw new InvalidOperationException($"Department with name '{createDto.Name}' already exists.");
        }

        var department = createDto.Adapt<Department>();
        await _departmentRepository.AddAsync(department);
        await _unitOfWork.CompleteAsync();

        return department.Adapt<DepartmentDto>();
    }

    public async Task<DepartmentDto?> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto)
    {
        var spec = new GetDepartmentByIdSpecification(id);
        var department = await _departmentRepository.GetEntityWithSpec(spec);
        
        if (department == null)
        {
            return null;
        }

        var existingSpec = new GetDepartmentByNameSpecification(updateDto.Name);
        var existing = await _departmentRepository.GetEntityWithSpec(existingSpec);
        
        if (existing != null && existing.Id != id)
        {
            throw new InvalidOperationException($"Department with name '{updateDto.Name}' already exists.");
        }

        department.Name = updateDto.Name;
        department.Description = updateDto.Description;
        
        _departmentRepository.Update(department);
        await _unitOfWork.CompleteAsync();

        return department.Adapt<DepartmentDto>();
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        var spec = new GetDepartmentByIdSpecification(id);
        var department = await _departmentRepository.GetEntityWithSpec(spec);
        
        if (department == null)
        {
            return false;
        }

        await _departmentRepository.DeleteAsync(department);
        await _unitOfWork.CompleteAsync();

        return true;
    }
}
