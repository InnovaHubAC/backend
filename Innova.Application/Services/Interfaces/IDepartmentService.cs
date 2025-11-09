namespace Innova.Application.Services.Interfaces;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> GetAllDepartmentsAsync();
    Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto createDto);
    Task<DepartmentDto?> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto);
    Task<bool> DeleteDepartmentAsync(int id);
}
