using Innova.API.Helpers;
using Innova.API.Requests.Idea;
using Innova.Application.DTOs;
using Innova.Application.DTOs.Common;
using Innova.Application.DTOs.Idea;
using Innova.Application.Validations.Idea;
using Mapster;

namespace Innova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdeasController : ControllerBase
    {
        private readonly IIdeaService _ideaService;

        public IdeasController(IIdeaService ideaService)
        {
            _ideaService = ideaService;
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

    }
}
