using Innova.Application.Validations.Users;

namespace Innova.Application.Services.Implementations
{
    public class UsersService : IUsersService
    {
        private readonly IIdentityService _identityService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UsersService> _logger;

        public UsersService(
            IIdentityService identityService,
            ICacheService cacheService,
            ILogger<UsersService> logger)
        {
            _identityService = identityService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginationDto<UserDto>>> GetUsersPagedAsync(int pageIndex, int pageSize, string? sort)
        {
            var cacheKey = $"User:GetPaged:Page{pageIndex}:Size{pageSize}:Sort{sort ?? "default"}";
            
            var cachedResult = _cacheService.Get<ApiResponse<PaginationDto<UserDto>>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation(
                    "Cache hit for users list. CacheKey: {CacheKey}, PageIndex: {PageIndex}, PageSize: {PageSize}",
                    cacheKey,
                    pageIndex,
                    pageSize);
                return cachedResult;
            }

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

            var response = ApiResponse<PaginationDto<UserDto>>.Success(pagination);
            
            _cacheService.Set(cacheKey, response, _cacheService.SetMemoryCacheEntryOptions(TimeSpan.FromMinutes(5)));

            _logger.LogInformation(
                "Users list retrieved successfully. TotalCount: {TotalCount}, PageIndex: {PageIndex}, PageSize: {PageSize}, Sort: {Sort}",
                totalCount,
                pageIndex,
                pageSize,
                sort ?? "default");

            return response;
        }

        public async Task<ApiResponse<DeletedDto>> DeleteUserAsync(string userId)
        {
            var result = await _identityService.DeleteUserAsync(userId);
            
            if (result)
            {
                _logger.LogInformation(
                    "User deleted successfully. UserId: {UserId}",
                    userId);

                InvalidateUserListCache();
                return ApiResponse<DeletedDto>.Success(new DeletedDto { IsDeleted = true });
            }
            else
            {
                _logger.LogWarning(
                    "Delete failed. User not found or deletion failed. UserId: {UserId}",
                    userId);

                return new ApiResponse<DeletedDto>(404, new DeletedDto { IsDeleted = false }, "User not found or deletion failed");
            }
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

                _logger.LogWarning(
                    "User update validation failed. UserId: {UserId}, ValidationErrors: {@ValidationErrors}",
                    userId,
                    errors);

                return new ApiResponse<UpdatedDto>(400, new UpdatedDto { IsUpdated = false }, string.Join("; ", errors));
            }

            var result = await _identityService.UpdateUserAsync(userId, firstName, lastName, dateOfBirth);
            
            if (result)
            {
                _logger.LogInformation(
                    "User updated successfully. UserId: {UserId}, Changes: {@Changes}",
                    userId,
                    new { FirstName = firstName, LastName = lastName, DateOfBirth = dateOfBirth });

                InvalidateUserListCache();
                return ApiResponse<UpdatedDto>.Success(new UpdatedDto { IsUpdated = true });
            }
            else
            {
                _logger.LogWarning(
                    "Update failed. User not found or update failed. UserId: {UserId}",
                    userId);

                return new ApiResponse<UpdatedDto>(404, new UpdatedDto { IsUpdated = false }, "User not found or update failed");
            }
        }

        private void InvalidateUserListCache()
        {
            _cacheService.RemoveByPrefix("User:GetPaged:");
            _logger.LogInformation(
                "Cache invalidated for prefix {CachePrefix}. Reason: {Reason}",
                "User:GetPaged:",
                "User data modified");
        }
    }
}