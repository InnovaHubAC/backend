using Innova.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);

            if (!response.Data!.IsAuthenticated)
                return BadRequest(response);

            _jwtTokenService.SetTokenCookieAsHttpOnly("InnovaRefreshToken", response.Data.RefreshToken!, response.Data.RefreshTokenExpiresOn);
            return Ok(response);

        }

        [HttpPost]
        [Route("Login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            if (!response.Data!.IsAuthenticated)
                return BadRequest(response);

            _jwtTokenService.SetTokenCookieAsHttpOnly("InnovaRefreshToken", response.Data.RefreshToken!, response.Data.RefreshTokenExpiresOn);
            return Ok(response);
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

            var response = await _authService.RefreshToken(refreshToken);
            if (!response.Data!.IsAuthenticated)
                return BadRequest(response);

            _jwtTokenService.SetTokenCookieAsHttpOnly("InnovaRefreshToken", response.Data.RefreshToken!, response.Data.RefreshTokenExpiresOn);
            return Ok(response);
        }

        [HttpPost]
        [Route("verify-email")]
        [ProducesResponseType(typeof(ApiResponse<VerifyEmailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VerifyEmailResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<VerifyEmailResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VerifyEmailResponseDto>>> VerifyEmail(VerifyEmailDto verifyEmailDto)
        {
            var response = await _authService.VerifyEmailAsync(verifyEmailDto);

            if (!response.Data!.IsVerified)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost]
        [Route("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PasswordResetResponseDto>>> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var response = await _authService.ForgotPasswordAsync(forgotPasswordDto);

            if (!response.Data!.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost]
        [Route("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PasswordResetResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PasswordResetResponseDto>>> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var response = await _authService.ResetPasswordAsync(resetPasswordDto);

            if (!response.Data!.IsSuccess)
                return BadRequest(response);

            return Ok(response);
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

            if (apiResponse.StatusCode != 200)
            {
                return BadRequest(apiResponse);
            }

            // TODO: Must be changed to (only) redirect to returnUrl since this may return json response
            if (string.IsNullOrEmpty(returnUrl))
            {
                return Ok(apiResponse);
            }
            else
            {
                return Redirect(returnUrl);
            }
        }
    }
}
