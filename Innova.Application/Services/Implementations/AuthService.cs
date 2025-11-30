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

        // Public methods first
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var registrationErrors = await ValidateRegistrationAsync(registerDto);
            if (registrationErrors.Any())
            {
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Message = "Registration failed.",
                    Errors = registrationErrors
                };
            }

            var userCreationError = await CreateUserWithRoleAsync(registerDto);
            if (userCreationError is not null && userCreationError.Any())
            {
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Message = "User creation failed.",
                    Errors = userCreationError
                };
            }

            // TODO: use background job to send email
            await SendEmailConfirmationAsync(registerDto.Email, registerDto.UserName);

            var response = await GenerateAuthResponseAsync(registerDto.UserName, registerDto.Email);
            return response;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Validate user credentials
            if (!await _identityService.ValidateUserCredentialsAsync(loginDto.Email, loginDto.Password))
            {
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Message = "Invalid email or password."
                };
            }

            if (!await _identityService.IsEmailConfirmedAsync(loginDto.Email))
            {
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Message = "Email is not confirmed. Please verify your email before logging in."
                };
            }

            var response = await CreateAuthResponseForLoginAsync(loginDto);
            return response;
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshToken(string token)
        {
            if (!await _jwtTokenService.ValidateRefreshTokenAsync(token))
            {
                return new ApiResponse<AuthResponseDto>(400, null, "Invalid refresh token.");
            }
            var response = await GenerateAuthResponseFromRefreshTokenAsync(token);
            return new ApiResponse<AuthResponseDto>(200, response, "Token refreshed successfully.");
        }

        public async Task<ApiResponse<VerifyEmailResponseDto>> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
        {
            var validator = new VerifyEmailDtoValidator();
            var validationResult = validator.Validate(verifyEmailDto);

            if (!validationResult.IsValid)
            {
                return new ApiResponse<VerifyEmailResponseDto>(400, null, "Validation failed.", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var tokenBytes = Convert.FromBase64String(verifyEmailDto.Token);
            var token = System.Text.Encoding.UTF8.GetString(tokenBytes);
            var isConfirmed = await _identityService.ConfirmEmailAsync(verifyEmailDto.Email, token);

            if (!isConfirmed)
            {
                return new ApiResponse<VerifyEmailResponseDto>(400, null, "Email verification failed.");
            }

            var response = new VerifyEmailResponseDto
            {
                IsVerified = true
            };
            return new ApiResponse<VerifyEmailResponseDto>(200, response, "Email verified successfully.");
        }

        public async Task<ApiResponse<PasswordResetResponseDto>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var validator = new ForgotPasswordDtoValidator();
            var validationResult = validator.Validate(forgotPasswordDto);

            if (!validationResult.IsValid)
            {
                return new ApiResponse<PasswordResetResponseDto>(400, null, "Validation failed.", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            if (!await _identityService.EmailExistsAsync(forgotPasswordDto.Email))
            {
                var response = new PasswordResetResponseDto
                {
                    IsSuccess = true
                };
                return new ApiResponse<PasswordResetResponseDto>(200, response, "If your email exists in our system, you will receive a password reset link shortly.");
            }

            var resetToken = await _identityService.GeneratePasswordResetTokenAsync(forgotPasswordDto.Email);
            if (string.IsNullOrEmpty(resetToken))
            {
                return new ApiResponse<PasswordResetResponseDto>(500, null, "Failed to generate password reset token.");
            }

            var userName = await _identityService.GetUserNameByEmailAsync(forgotPasswordDto.Email);
            // TODO: use background job to send email
            await _emailService.SendPasswordResetEmailAsync(forgotPasswordDto.Email, userName, resetToken);

            var successResponse = new PasswordResetResponseDto
            {
                IsSuccess = true
            };
            return new ApiResponse<PasswordResetResponseDto>(200, successResponse, "If your email exists in our system, you will receive a password reset link shortly.");
        }

        public async Task<ApiResponse<PasswordResetResponseDto>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var validator = new ResetPasswordDtoValidator();
            var validationResult = validator.Validate(resetPasswordDto);

            if (!validationResult.IsValid)
            {
                return new ApiResponse<PasswordResetResponseDto>(400, null, "Validation failed.", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var isReset = await _identityService.ResetPasswordAsync(
                resetPasswordDto.Email,
                resetPasswordDto.Token,
                resetPasswordDto.NewPassword);

            if (!isReset)
            {
                return new ApiResponse<PasswordResetResponseDto>(400, null, "Password reset failed. Invalid token or email.");
            }

            var response = new PasswordResetResponseDto
            {
                IsSuccess = true
            };
            return new ApiResponse<PasswordResetResponseDto>(200, response, "Password reset successfully. You can now login with your new password.");
        }

        public async Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(ClaimsPrincipal principal)
        {
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var googleId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var firstName = principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = principal.FindFirstValue(ClaimTypes.Surname);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
            {
                return new ApiResponse<AuthResponseDto>(400, null, "Failed to retrieve user information from Google.");
            }

            var userName = await GetUserNameByProviderAsync("Google", googleId);

            if (userName == null)
            {
                if (await EmailExistsAsync(email))
                {
                    return new ApiResponse<AuthResponseDto>(400, null, "An account with this email already exists. Please login with your password.");
                }

                userName = email.Split('@')[0]; // assuming username is the part before '@'
                var createResult = await CreateExternalUserAsync(email, userName, firstName ?? "", lastName ?? "", "Google", googleId);
                if (!createResult.Success)
                {
                    return new ApiResponse<AuthResponseDto>(400, null, "Failed to create user.");
                }
            }

            var result = await GenerateAuthResponseForExternalLoginAsync(userName, email);
            _jwtTokenService.SetTokenCookieAsHttpOnly("InnovaRefreshToken", result.RefreshToken!, result.RefreshTokenExpiresOn);

            return new ApiResponse<AuthResponseDto>(200, result, "Login successful.");
        }

        public async Task<string?> GetUserNameByProviderAsync(string provider, string providerKey)
        {
            return await _identityService.GetUserNameByProviderAsync(provider, providerKey);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _identityService.EmailExistsAsync(email);
        }

        public async Task<(bool Success, List<string> Errors)> CreateExternalUserAsync(string email, string userName, string firstName, string lastName, string provider, string providerKey)
        {
            var result = await _identityService.CreateExternalUserAsync(email, userName, firstName, lastName, provider, providerKey);

            if (result.Success)
            {
                // Add default role to the new user
                var roleErrors = await _identityService.AddToRoleAsync(userName, DefaultUserRole);
                if (roleErrors.Any())
                {
                    return (false, roleErrors);
                }
            }

            return result;
        }

        public async Task<AuthResponseDto> GenerateAuthResponseForExternalLoginAsync(string userName, string email)
        {
            var jwtToken = await _jwtTokenService.CreateTokenAsync(userName);
            var refreshTokenResult = await _jwtTokenService.CreateRefreshTokenAsync(userName);

            var (refreshToken, refreshTokenExpiry) = refreshTokenResult.Value;

            return new AuthResponseDto
            {
                IsAuthenticated = true,
                Token = jwtToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiresOn = refreshTokenExpiry,
                UserName = userName,
                Email = email
            };
        }

        // Private methods after public
        private async Task<List<string>> ValidateRegistrationAsync(RegisterDto registerDto)
        {
            List<string> errors = new List<string>();

            var validator = new RegisterDtoValidator();
            var validationResult = validator.Validate(registerDto);

            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
                return errors;
            }

            if (await _identityService.EmailExistsAsync(registerDto.Email))
            {
                errors.Add("Email is already registered.");
                return errors;
            }

            if (await _identityService.UserNameExistsAsync(registerDto.UserName))
            {
                errors.Add("Username is already taken.");
                return errors;
            }

            return errors;
        }

        private async Task<List<string>> CreateUserWithRoleAsync(RegisterDto registerDto)
        {
            var userCreationErrors = await _identityService.CreateUserAsync(
                registerDto.FirstName,
                registerDto.LastName,
                registerDto.Email,
                registerDto.DateOfBirth,
                registerDto.UserName,
                registerDto.Password);

            if (userCreationErrors.Any())
                return userCreationErrors;

            var roleAssignmentErrors = await _identityService.AddToRoleAsync(registerDto.UserName, DefaultUserRole);
            if (roleAssignmentErrors.Any())
                return roleAssignmentErrors;

            return new();
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
                IsAuthenticated = true,
                Token = jwtToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiresOn = refreshTokenExpiry,
                UserName = userName,
                Email = email
            };
        }

        private async Task<AuthResponseDto> CreateAuthResponseForLoginAsync(LoginDto loginDto)
        {
            var userName = await _identityService.GetUserNameByEmailAsync(loginDto.Email);
            var jwtToken = await _jwtTokenService.CreateTokenAsync(userName);
            var refreshToken = await _jwtTokenService.GetActiveRefreshToken(loginDto.Email);
            return new AuthResponseDto
            {
                IsAuthenticated = true,
                Token = jwtToken,
                RefreshToken = refreshToken?.Item1,
                RefreshTokenExpiresOn = refreshToken?.Item2 ?? DateTime.UtcNow.AddDays(7),
                UserName = userName,
                Email = loginDto.Email
            };
        }

        private async Task<AuthResponseDto> GenerateAuthResponseFromRefreshTokenAsync(string token)
        {
            var userName = await _jwtTokenService.GetUserUserNameFromRefreshTokenAsync(token);
            var newJwtToken = await _jwtTokenService.CreateTokenAsync(userName);
            var refreshTokenExperationDate = await _jwtTokenService.GetRefreshTokenExpirationDate(token);
            return new AuthResponseDto
            {
                IsAuthenticated = true,
                Token = newJwtToken,
                RefreshToken = token,
                RefreshTokenExpiresOn = refreshTokenExperationDate,
                UserName = userName
            };
        }
    }
}
