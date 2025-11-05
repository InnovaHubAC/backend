using Innova.Application.DTOs.Auth;
using Innova.Application.Services.Interfaces;
using Innova.Application.Validations.Auth;

namespace Innova.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenService _jwtTokenService;
        private const string DefaultUserRole = "User";

        public AuthService(IIdentityService identityService, IJwtTokenService jwtTokenService)
        {
            _identityService = identityService;
            _jwtTokenService = jwtTokenService;
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

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(string userName, string email)
        {
            var jwtToken = await _jwtTokenService.CreateTokenAsync(userName);
            var refreshTokenResult = await _jwtTokenService.CreateRefreshTokenAsync(userName);
            
            var (refreshToken, refreshTokenExpiry) = refreshTokenResult.Value;

            return new AuthResponseDto
            {
                Message = "Registration successful.",
                IsAuthenticated = true,
                Token = jwtToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiresOn = refreshTokenExpiry,
                UserName = userName,
                Email = email,
            };
        }
    }
}
