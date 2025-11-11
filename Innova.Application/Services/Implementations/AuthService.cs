using Innova.Application.DTOs.Auth;
using Innova.Application.Services.Interfaces;
using Innova.Application.Validations.Auth;

namespace Innova.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private const string DefaultUserRole = "User";

        public AuthService(IIdentityService identityService, IJwtTokenService jwtTokenService, IEmailService emailService)
        {
            _identityService = identityService;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var validationError = await ValidateRegistrationAsync(registerDto);
            if (validationError != null)
            {
                return validationError;
            }

            var userCreationError = await CreateUserWithRoleAsync(registerDto);
            if (userCreationError != null)
            {
                return userCreationError;
            }

            // Send email confirmation
            await SendEmailConfirmationAsync(registerDto.Email, registerDto.UserName);

            return await GenerateAuthResponseAsync(registerDto.UserName, registerDto.Email);
        }

        private async Task<AuthResponseDto?> ValidateRegistrationAsync(RegisterDto registerDto)
        {
            var validator = new RegisterDtoValidator();
            var validationResult = validator.Validate(registerDto);

            if (!validationResult.IsValid)
            {
                return new AuthResponseDto
                {
                    Message = "Registration failed.",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList(),
                };
            }

            if (await _identityService.EmailExistsAsync(registerDto.Email))
            {
                return new AuthResponseDto
                {
                    Message = "Email already in use.",
                };
            }

            if (await _identityService.UserNameExistsAsync(registerDto.UserName))
            {
                return new AuthResponseDto
                {
                    Message = "Username already in use.",
                };
            }

            return null;
        }

        private async Task<AuthResponseDto?> CreateUserWithRoleAsync(RegisterDto registerDto)
        {
            var userCreationErrors = await _identityService.CreateUserAsync(
                registerDto.FirstName,
                registerDto.LastName,
                registerDto.Email,
                registerDto.DateOfBirth,
                registerDto.UserName,
                registerDto.Password);

            if (userCreationErrors.Any())
            {
                return new AuthResponseDto
                {
                    Message = "User creation failed.",
                    Errors = userCreationErrors
                };
            }

            var roleAssignmentErrors = await _identityService.AddToRoleAsync(registerDto.UserName, DefaultUserRole);
            if (roleAssignmentErrors.Any())
            {
                return new AuthResponseDto
                {
                    Message = "Assigning role failed.",
                    Errors = roleAssignmentErrors
                };
            }

            return null;
        }

        private async Task SendEmailConfirmationAsync(string email, string userName)
        {
            var confirmationToken = await _identityService.GenerateEmailConfirmationTokenAsync(email);
            if (!string.IsNullOrEmpty(confirmationToken))
            {
                await _emailService.SendRegisterationEmailConfirmationAsync(email, userName, confirmationToken);
            }
        }

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(string userName, string email)
        {
            var jwtToken = await _jwtTokenService.CreateTokenAsync(userName);
            var refreshTokenResult = await _jwtTokenService.CreateRefreshTokenAsync(userName);

            var (refreshToken, refreshTokenExpiry) = refreshTokenResult.Value;

            return new AuthResponseDto
            {
                Message = "Registration successful. Please check your email to verify your account.",
                IsAuthenticated = true,
                Token = jwtToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiresOn = refreshTokenExpiry,
                UserName = userName,
                Email = email,
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Validate user credentials
            if (!await _identityService.ValidateUserCredentialsAsync(loginDto.Email, loginDto.Password))
            {
                return new AuthResponseDto
                {
                    Message = "Invalid email or password.",
                };
            }

            if(!await _identityService.IsEmailConfirmedAsync(loginDto.Email))
            {
                return new AuthResponseDto
                {
                    Message = "Email is not confirmed. Please verify your email before logging in.",
                };
            }

            return await CreateAuthResponseForLoginAsync(loginDto);
        }

        private async Task<AuthResponseDto> CreateAuthResponseForLoginAsync(LoginDto loginDto)
        {
            var userName = await _identityService.GetUserNameByEmailAsync(loginDto.Email);
            var jwtToken = await _jwtTokenService.CreateTokenAsync(userName);
            var refreshToken = await _jwtTokenService.GetActiveRefreshToken(loginDto.Email);
            return new AuthResponseDto
            {
                Message = "Login successful.",
                IsAuthenticated = true,
                Token = jwtToken,
                RefreshToken = refreshToken?.Item1,
                RefreshTokenExpiresOn = refreshToken?.Item2 ?? DateTime.UtcNow.AddDays(7),
                UserName = userName,
                Email = loginDto.Email,
            };
        }

        public async Task<AuthResponseDto> RefreshToken(string token)
        {
            if (!await _jwtTokenService.ValidateRefreshTokenAsync(token))
            {
                return new AuthResponseDto
                {
                    Message = "Invalid refresh token.",
                };
            }
            return await GenerateAuthResponseFromRefreshTokenAsync(token);
        }

        private async Task<AuthResponseDto> GenerateAuthResponseFromRefreshTokenAsync(string token)
        {
            var userName = await _jwtTokenService.GetUserUserNameFromRefreshTokenAsync(token);
            var newJwtToken = await _jwtTokenService.CreateTokenAsync(userName);
            var refreshTokenExperationDate = await _jwtTokenService.GetRefreshTokenExpirationDate(token);
            return new AuthResponseDto
            {
                Message = "Token refreshed successfully.",
                IsAuthenticated = true,
                Token = newJwtToken,
                RefreshToken = token,
                RefreshTokenExpiresOn = refreshTokenExperationDate,
                UserName = userName,
            };
        }

        public async Task<VerifyEmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
        {
            var validator = new VerifyEmailDtoValidator();
            var validationResult = validator.Validate(verifyEmailDto);

            if (!validationResult.IsValid)
            {
                return new VerifyEmailResponseDto
                {
                    IsVerified = false,
                    Message = "Validation failed.",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList(),
                };
            }

            var isConfirmed = await _identityService.ConfirmEmailAsync(verifyEmailDto.Email, verifyEmailDto.Token);

            if (!isConfirmed)
            {
                return new VerifyEmailResponseDto
                {
                    IsVerified = false,
                    Message = "Email verification failed. Invalid token or email.",
                };
            }

            return new VerifyEmailResponseDto
            {
                IsVerified = true,
                Message = "Email verified successfully.",
            };
        }

        public async Task<PasswordResetResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var validator = new ForgotPasswordDtoValidator();
            var validationResult = validator.Validate(forgotPasswordDto);

            if (!validationResult.IsValid)
            {
                return new PasswordResetResponseDto
                {
                    IsSuccess = false,
                    Message = "Validation failed.",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList(),
                };
            }

            if (!await _identityService.EmailExistsAsync(forgotPasswordDto.Email))
            {
                return new PasswordResetResponseDto
                {
                    IsSuccess = true,
                    Message = "If your email exists in our system, you will receive a password reset link shortly.",
                };
            }

            var resetToken = await _identityService.GeneratePasswordResetTokenAsync(forgotPasswordDto.Email);
            if (string.IsNullOrEmpty(resetToken))
            {
                return new PasswordResetResponseDto
                {
                    IsSuccess = false,
                    Message = "Failed to generate password reset token.",
                };
            }

            var userName = await _identityService.GetUserNameByEmailAsync(forgotPasswordDto.Email);
            await _emailService.SendPasswordResetEmailAsync(forgotPasswordDto.Email, userName, resetToken);

            return new PasswordResetResponseDto
            {
                IsSuccess = true,
                Message = "If your email exists in our system, you will receive a password reset link shortly.",
            };
        }

        public async Task<PasswordResetResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var validator = new ResetPasswordDtoValidator();
            var validationResult = validator.Validate(resetPasswordDto);

            if (!validationResult.IsValid)
            {
                return new PasswordResetResponseDto
                {
                    IsSuccess = false,
                    Message = "Validation failed.",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList(),
                };
            }

            var isReset = await _identityService.ResetPasswordAsync(
                resetPasswordDto.Email,
                resetPasswordDto.Token,
                resetPasswordDto.NewPassword);

            if (!isReset)
            {
                return new PasswordResetResponseDto
                {
                    IsSuccess = false,
                    Message = "Password reset failed. Invalid token or email.",
                };
            }

            return new PasswordResetResponseDto
            {
                IsSuccess = true,
                Message = "Password reset successfully. You can now login with your new password.",
            };
        }
    }
}
