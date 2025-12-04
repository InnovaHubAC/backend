namespace Innova.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<AuthResponseDto>> RefreshToken(string token);
        Task<ApiResponse<VerifyEmailResponseDto>> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
        Task<ApiResponse<PasswordResetResponseDto>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ApiResponse<PasswordResetResponseDto>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(ClaimsPrincipal principal);
        Task<string?> GetUserNameByProviderAsync(string provider, string providerKey);
        Task<bool> EmailExistsAsync(string email);
        Task<(bool Success, List<string> Errors)> CreateExternalUserAsync(string email, string userName, string firstName, string lastName, string provider, string providerKey);
        Task<AuthResponseDto> GenerateAuthResponseForExternalLoginAsync(string userName, string email);
    }
}
