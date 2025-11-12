using Innova.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Innova.Domain.Interfaces;

namespace Innova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost]
        [Route("Register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (!result.IsAuthenticated)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail(400, result.Message, result.Errors));

            _jwtTokenService.SetTokenCookieAsHttpOnly("InnovaRefreshToken", result.RefreshToken!, result.RefreshTokenExpiresOn);
            return Ok(ApiResponse<AuthResponseDto>.Success(result));

        }

        [HttpPost]
        [Route("Login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (!result.IsAuthenticated)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail(400, result.Message, result.Errors));

            _jwtTokenService.SetTokenCookieAsHttpOnly("InnovaRefreshToken", result.RefreshToken!, result.RefreshTokenExpiresOn);
            return Ok(ApiResponse<AuthResponseDto>.Success(result));
        }

        [HttpGet]
        [Route("RefreshToken")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("InnovaRefreshToken", out var refreshToken))
            {
                return BadRequest(ApiResponse<AuthResponseDto>.Fail(400, "Refresh token is missing."));
            }
            var result = await _authService.RefreshToken(refreshToken);
            if (!result.IsAuthenticated)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail(400, result.Message, result.Errors));

            _jwtTokenService.SetTokenCookieAsHttpOnly("InnovaRefreshToken", result.RefreshToken!, result.RefreshTokenExpiresOn);
            return Ok(ApiResponse<AuthResponseDto>.Success(result));
        }

        [HttpPost]
        [Route("verify-email")]
        [ProducesResponseType(typeof(ApiResponse<VerifyEmailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VerifyEmailResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<VerifyEmailResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VerifyEmailResponseDto>>> VerifyEmail(VerifyEmailDto verifyEmailDto)
        {
            var result = await _authService.VerifyEmailAsync(verifyEmailDto);

            if (!result.IsVerified)
                return BadRequest(ApiResponse<VerifyEmailResponseDto>.Fail(400, result.Message, result.Errors));

            return Ok(ApiResponse<VerifyEmailResponseDto>.Success(result));
        }

        [HttpPost]
        [Route("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PasswordResetResponseDto>>> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PasswordResetResponseDto>.Fail(400, result.Message, result.Errors));

            return Ok(ApiResponse<PasswordResetResponseDto>.Success(result));
        }

        [HttpPost]
        [Route("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PasswordResetResponseDto>>> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PasswordResetResponseDto>.Fail(400, result.Message, result.Errors));

            return Ok(ApiResponse<PasswordResetResponseDto>.Success(result));
        }

        [HttpGet]
        [Route("google-login")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl });
            var properties = new AuthenticationProperties 
            { 
                RedirectUri = redirectUrl 
            };
            return Challenge(properties, "Google");
        }

        [HttpGet]
        [Route("google-callback")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GoogleCallback([FromQuery] string? returnUrl = null)
        {
            var info = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            if (!info.Succeeded)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail(401, "Google authentication failed."));
            }

            var apiResponse = await _authService.GoogleLoginAsync(info.Principal);

            if(apiResponse.StatusCode != 200) { 
                return BadRequest(apiResponse);
            }

            
            // TODO: Redirect to returnUrl if needed            
            return Ok(apiResponse);
        }
    }
}
