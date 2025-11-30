namespace Innova.Application.Services.Interfaces
{
    public interface IIdeaService
    {
        Task<ApiResponse<bool>> CreateIdeaAsync(CreateIdeaDto createIdeaDto);
        Task<ApiResponse<bool>> UpdateIdeaAsync(UpdateIdeaDto updateIdeaDto);
        Task<ApiResponse<IdeaDetailsDto>> GetIdeaDetailsAsync(int ideaId);
        Task<ApiResponse<bool>> DeleteIdeaAsync(int ideaId, string userId);
        Task<ApiResponse<PaginationDto<IdeaDetailsDto>>> GetIdeasByUserIdAsync(string userId, PaginationParams paginationParams);
    }
}
