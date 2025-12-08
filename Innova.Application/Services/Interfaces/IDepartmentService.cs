namespace Innova.Application.Services.Interfaces;

public interface IDepartmentService
{
    Task<ApiResponse<PaginationDto<DepartmentDto>>> GetAllDepartmentsAsync(PaginationParams paginationParams);
    Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id);
    Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto createDto);
    Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto);
    Task<ApiResponse<DeletedDto>> DeleteDepartmentAsync(int id);
}
