using Innova.Application.DTOs.Users;

namespace Innova.Application.Services.Interfaces
{
    public interface IUsersService
    {
        Task<ApiResponse<PaginationDto<UserDto>>> GetUsersPagedAsync(int pageIndex, int pageSize, string? sort);
        Task<ApiResponse<UpdatedDto>> UpdateUserAsync(string userId, string firstName, string lastName, DateTime? dateOfBirth);
        Task<ApiResponse<DeletedDto>> DeleteUserAsync(string userId);
    }
}