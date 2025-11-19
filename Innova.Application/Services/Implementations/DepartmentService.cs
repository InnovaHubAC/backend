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

    public async Task<ApiResponse<IReadOnlyList<DepartmentDto>>> GetAllDepartmentsAsync()
    {
        var spec = new GetAllDepartmentsSpecification();
        var departments = await _departmentRepository.ListAsync(spec);
        var dtos = departments.Adapt<IReadOnlyList<DepartmentDto>>();
        return new ApiResponse<IReadOnlyList<DepartmentDto>>(200, dtos, "Departments retrieved successfully.");
    }

    public async Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id)
    {
        var spec = new GetDepartmentByIdSpecification(id);
        var department = await _departmentRepository.GetEntityWithSpec(spec);
        if (department == null)
        {
            return new ApiResponse<DepartmentDto>(404, null, $"Department with id {id} not found.");
        }
        var dto = department.Adapt<DepartmentDto>();
        return new ApiResponse<DepartmentDto>(200, dto, "Department retrieved successfully.");
    }

    public async Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto createDto)
    {
        var existingSpec = new GetDepartmentByNameSpecification(createDto.Name);
        var existing = await _departmentRepository.GetEntityWithSpec(existingSpec);
        if (existing != null)
        {
            return new ApiResponse<DepartmentDto>(400, null, $"Department with name '{createDto.Name}' already exists.");
        }

        var department = createDto.Adapt<Department>();
        await _departmentRepository.AddAsync(department);
        await _unitOfWork.CompleteAsync();

        var dto = department.Adapt<DepartmentDto>();
        return new ApiResponse<DepartmentDto>(201, dto, "Department created successfully.");
    }

    public async Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto)
    {
        var spec = new GetDepartmentByIdSpecification(id);
        var department = await _departmentRepository.GetEntityWithSpec(spec);
        if (department == null)
        {
            return new ApiResponse<DepartmentDto>(404, null, $"Department with id {id} not found.");
        }

        var existingSpec = new GetDepartmentByNameSpecification(updateDto.Name);
        var existing = await _departmentRepository.GetEntityWithSpec(existingSpec);
        if (existing != null && existing.Id != id)
        {
            return new ApiResponse<DepartmentDto>(400, null, $"Department with name '{updateDto.Name}' already exists.");
        }

        department.Name = updateDto.Name;
        department.Description = updateDto.Description;

        _departmentRepository.Update(department);
        await _unitOfWork.CompleteAsync();

        var dto = department.Adapt<DepartmentDto>();
        return new ApiResponse<DepartmentDto>(200, dto, "Department updated successfully.");
    }

    public async Task<ApiResponse<DeletedDto>> DeleteDepartmentAsync(int id)
    {
        var spec = new GetDepartmentByIdSpecification(id);
        var department = await _departmentRepository.GetEntityWithSpec(spec);
        if (department == null)
        {
            return new ApiResponse<DeletedDto>(404, new DeletedDto { IsDeleted = false }, $"Department with id {id} not found.");
        }

        await _departmentRepository.DeleteAsync(department);
        await _unitOfWork.CompleteAsync();

        return new ApiResponse<DeletedDto>(200, new DeletedDto { IsDeleted = true }, "Department deleted successfully.");
    }
}
