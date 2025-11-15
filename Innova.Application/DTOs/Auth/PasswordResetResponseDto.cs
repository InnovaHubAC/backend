namespace Innova.Application.DTOs.Auth;

public class PasswordResetResponseDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
}
