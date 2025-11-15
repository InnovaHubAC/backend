using Innova.API.Helpers;
using Innova.API.Requests.Idea;
using Innova.Application.DTOs;
using Innova.Application.DTOs.Common;
using Innova.Application.DTOs.Idea;
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
    }
}
