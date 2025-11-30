using Innova.API.Helpers;
using Innova.API.Requests.Idea;
using Innova.Application.DTOs;
using Innova.Application.DTOs.Common;
using Innova.Application.DTOs.Idea;
using Innova.Application.DTOs.Comment;
using Innova.Application.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Innova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdeasController : ControllerBase
    {
        private readonly IIdeaService _ideaService;
        private readonly ICommentService _commentService;

        public IdeasController(IIdeaService ideaService, ICommentService commentService)
        {
            _ideaService = ideaService;
            _commentService = commentService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> CreateIdea([FromForm] CreateIdeaRequest createIdea)
        {
            CreateIdeaDto createIdeaDto = createIdea.Adapt<CreateIdeaDto>();
            createIdeaDto.Attachments = await FileHelper.ConvertFilesToAttachments(createIdea.Attachments!);
            var result = await _ideaService.CreateIdeaAsync(createIdeaDto);
            return result.StatusCode switch
            {
                200 => Ok(result),
                400 => BadRequest(result),
                404 => NotFound(result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateIdea([FromForm] UpdateIdeaRequest updateIdea)
        {
            UpdateIdeaDto updateIdeaDto = updateIdea.Adapt<UpdateIdeaDto>();
            updateIdeaDto.Attachments = await FileHelper.ConvertFilesToAttachments(updateIdea.Attachments!);
            var result = await _ideaService.UpdateIdeaAsync(updateIdeaDto);
            return result.StatusCode switch
            {
                200 => Ok(result),
                400 => BadRequest(result),
                404 => NotFound(result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        [HttpGet]
        [Route("{ideaId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IdeaDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IdeaDetailsDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<IdeaDetailsDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<IdeaDetailsDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IdeaDetailsDto>>> GetIdeaDetails([FromRoute] int ideaId)
        {
            var result = await _ideaService.GetIdeaDetailsAsync(ideaId);
            return result.StatusCode switch
            {
                200 => Ok(result),
                404 => NotFound(result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        [HttpDelete]
        [Authorize]
        [Route("{ideaId:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteIdea([FromRoute] int ideaId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<bool>.Fail(401, "User not authenticated"));
            var result = await _ideaService.DeleteIdeaAsync(ideaId, userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{ideaId}/comments")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<IEnumerable<CommentDto>>>> GetCommentsByIdeaId(int ideaId)
        {
            var result = await _commentService.GetCommentsByIdeaIdAsync(ideaId);
            return Ok(result);
        }

        [HttpPost("{ideaId}/comments")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CommentDto>>> CreateComment(int ideaId, [FromBody] CreateCommentDto createCommentDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _commentService.CreateCommentAsync(ideaId, createCommentDto, userId);
            return result.StatusCode switch
            {
                200 => Ok(result),
                404 => NotFound(result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

    }
}
