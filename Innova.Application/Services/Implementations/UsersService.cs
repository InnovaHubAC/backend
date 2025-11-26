using Innova.Application.Validations.Users;

namespace Innova.Application.Services.Implementations
{
    public class UsersService : IUsersService
    {
        private readonly IIdentityService _identityService;

        public UsersService(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<ApiResponse<PaginationDto<UserDto>>> GetUsersPagedAsync(int pageIndex, int pageSize, string? sort)
        {
            var (items, totalCount) = await _identityService.GetUsersPagedAsync(pageIndex, pageSize, sort);
            var dtos = items.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName ?? "",
                Email = u.Email ?? "",
                FirstName = u.FirstName ?? "",
                LastName = u.LastName ?? "",
                DateOfBirth = u.DateOfBirth,
            }).ToList();

            var pagination = new PaginationDto<UserDto>(pageIndex, pageSize, totalCount, dtos);

            return ApiResponse<PaginationDto<UserDto>>.Success(pagination);
        }

        public async Task<ApiResponse<DeletedDto>> DeleteUserAsync(string userId)
        {
            var result = await _identityService.DeleteUserAsync(userId);
            if (result)
                return ApiResponse<DeletedDto>.Success(new DeletedDto { IsDeleted = true });
            else
                return new ApiResponse<DeletedDto>(404, new DeletedDto { IsDeleted = false }, "User not found or deletion failed");
        }

        public async Task<ApiResponse<UpdatedDto>> UpdateUserAsync(string userId, string firstName, string lastName, DateTime? dateOfBirth)
        {
            var validator = new UpdateUserDtoValidator();
            var validationResult = await validator.ValidateAsync(new UpdateUserDto
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth
            });

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return new ApiResponse<UpdatedDto>(400, new UpdatedDto { IsUpdated = false }, string.Join("; ", errors));
            }

            var result = await _identityService.UpdateUserAsync(userId, firstName, lastName, dateOfBirth);
            if (result)
                return ApiResponse<UpdatedDto>.Success(new UpdatedDto { IsUpdated = true });
            else
                return new ApiResponse<UpdatedDto>(404, new UpdatedDto { IsUpdated = false }, "User not found or update failed");
        }
    }
}