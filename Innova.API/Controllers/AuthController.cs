using Innova.Application.DTOs.Auth;
using Innova.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Innova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        private void SetTokenCookie(string token, DateTime expirationDate)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expirationDate.ToLocalTime()
            };
            Response.Cookies.Append("RefreshToken", token, cookieOptions);
        }

        [HttpPost]
        [Route("Register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (!result.IsAuthenticated)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail(400, result.Message, result.Errors));

            SetTokenCookie(result.RefreshToken!, result.RefreshTokenExpiresOn);
            return Ok(ApiResponse<AuthResponseDto>.Success(result));

        }
    }
}
