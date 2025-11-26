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
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly IIdeaService _ideaService;

    public UsersController(IUsersService usersService, IIdeaService ideaService)
    {
        _usersService = usersService;
        _ideaService = ideaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginationDto<UserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginationDto<UserDto>>>> GetAllUsers([FromQuery] PaginationParams paginationParams)
    {
        var response = await _usersService.GetUsersPagedAsync(paginationParams.PageIndex, paginationParams.PageSize, paginationParams.Sort);
        return Ok(response);
    }

    [HttpGet("{id}/ideas")]
    [ProducesResponseType(typeof(ApiResponse<PaginationDto<IdeaDetailsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaginationDto<IdeaDetailsDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaginationDto<IdeaDetailsDto>>>> GetUserIdeas(string id, [FromQuery] PaginationParams paginationParams)
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
    public async Task<ActionResult<ApiResponse<UpdatedDto>>> UpdateUser(string id, [FromBody] UpdateUserDto updateDto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId != id)
        {
            return Forbid();
        }

        var response = await _usersService.UpdateUserAsync(id, updateDto.FirstName, updateDto.LastName, updateDto.DateOfBirth);

        // TODO: I think we can return StatusCode always instead of switch case
        return response.StatusCode switch
        {
            200 => Ok(response),
            400 => BadRequest(response),
            404 => NotFound(response),
            _ => StatusCode(response.StatusCode, response)
        };
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DeletedDto>>> DeleteUser(string id)
    {
        var response = await _usersService.DeleteUserAsync(id);
        
        return response.StatusCode switch
        {
            200 => Ok(response),
            404 => NotFound(response),
            _ => StatusCode(response.StatusCode, response)
        };
    }
}
