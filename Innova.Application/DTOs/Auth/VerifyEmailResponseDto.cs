namespace Innova.Application.DTOs.Auth;

public class VerifyEmailResponseDto
{
  public bool IsVerified { get; set; }
  public string Message { get; set; } = string.Empty;
  public List<string>? Errors { get; set; }
}
