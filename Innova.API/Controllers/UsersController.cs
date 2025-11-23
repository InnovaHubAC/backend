using Innova.Application.DTOs.Common;
using Innova.Application.DTOs.Users;
using Innova.Application.DTOs.Idea;
using Innova.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Innova.Domain.Interfaces;

namespace Innova.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IIdeaService _ideaService;

    public UsersController(IIdentityService identityService, IIdeaService ideaService)
    {
        _identityService = identityService;
        _ideaService = ideaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginationDto<UserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginationDto<UserDto>>>> GetAllUsers([FromQuery] PaginationParams paginationParams)
    {
        var (items, totalCount) = await _identityService.GetUsersPagedAsync(
            paginationParams.PageIndex,
            paginationParams.PageSize,
            paginationParams.Sort
        );

        // I have to do this due to the external service returning tuples
        var dtos = items.Select(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName ?? "",
            Email = u.Email ?? "",
            FirstName = u.FirstName ?? "",
            LastName = u.LastName ?? "",
            DateOfBirth = u.DateOfBirth,
        }).ToList();

        var pagination = new PaginationDto<UserDto>(paginationParams.PageIndex, paginationParams.PageSize, totalCount, dtos);

        return Ok(new ApiResponse<PaginationDto<UserDto>>(200, pagination, "Users retrieved successfully"));
    }

    [HttpGet("{id}/ideas")]
    [ProducesResponseType(typeof(ApiResponse<PaginationDto<IReadOnlyList<IdeaDetailsDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaginationDto<IReadOnlyList<IdeaDetailsDto>>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaginationDto<IReadOnlyList<IdeaDetailsDto>>>>> GetUserIdeas(string id, PaginationParams paginationParams)
    {
        var result = await _ideaService.GetIdeasByUserIdAsync(id, paginationParams);
        if (result.StatusCode == 404) return NotFound(result);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateUser(string id, [FromBody] UpdateUserDto updateDto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId != id)
        {
            return Forbid();
        }

        var result = await _identityService.UpdateUserAsync(id, updateDto.FirstName, updateDto.LastName, updateDto.DateOfBirth);
        
        if (!result) return NotFound(ApiResponse<bool>.Fail(404, "User not found or update failed"));

        return Ok(ApiResponse<bool>.Success(true));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(string id)
    {
        var result = await _identityService.DeleteUserAsync(id);
        if (!result) return NotFound(ApiResponse<bool>.Fail(404, "User not found or delete failed"));

        return Ok(ApiResponse<bool>.Success(true));
    }
}
