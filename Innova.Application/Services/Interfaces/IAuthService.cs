using System.Security.Claims;
using Innova.Application.DTOs;
using Innova.Application.DTOs.Auth;

namespace Innova.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RefreshToken(string token);
        Task<VerifyEmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
        Task<PasswordResetResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<PasswordResetResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(ClaimsPrincipal principal);
        Task<string?> GetUserNameByProviderAsync(string provider, string providerKey);
        Task<bool> EmailExistsAsync(string email);
        Task<(bool Success, List<string> Errors)> CreateExternalUserAsync(string email, string userName, string firstName, string lastName, string provider, string providerKey);
        Task<AuthResponseDto> GenerateAuthResponseForExternalLoginAsync(string userName, string email);
    }
}
