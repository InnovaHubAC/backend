using System.Text.Json.Serialization;

namespace Innova.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; }
        public string Token { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; } = false;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [JsonIgnore] public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiresOn { get; set; }
    }
}
