using Innova.Application.DTOs.Idea;

namespace Innova.Application.Services.Interfaces
{
    public interface IIdeaService
    {
        public Task<ApiResponse<bool>> CreateIdeaAsync(CreateIdeaDto createIdeaDto);
    }
}
