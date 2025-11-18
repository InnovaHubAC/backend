using Innova.Application.DTOs.Idea;
using Innova.Application.Validations.Idea;

namespace Innova.Application.Services.Interfaces
{
    public interface IIdeaService
    {
        public Task<ApiResponse<bool>> CreateIdeaAsync(CreateIdeaDto createIdeaDto);
        public Task<ApiResponse<bool>> UpdateIdeaAsync(UpdateIdeaDto updateIdeaDto);
    }
}
