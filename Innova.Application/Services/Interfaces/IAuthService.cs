using Innova.Application.DTOs;
using Innova.Application.DTOs.Auth;

namespace Innova.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RefreshToken(string token);
    }
}
