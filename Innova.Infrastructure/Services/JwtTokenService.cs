using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Innova.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IIdentityService _identityService;
        private readonly UserManager<AppUser> _userManager;
        private readonly JwtSettings _jwt;


        public JwtTokenService(IIdentityService identityService, UserManager<AppUser> userManager, IOptions<JwtSettings> jwt)
        {
            _identityService = identityService;
            _userManager = userManager;
            _jwt = jwt.Value;
        }

        public async Task<string> CreateTokenAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return "No user found";

            var userClaims = await _identityService.GetAllUserClaimsAsync(userName);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, 15.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            }
            .Union(userClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpirationInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationInDays),
            };
        }

        public async Task<(string, DateTime)?> CreateRefreshTokenAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return null;
            var refreshToken = GenerateRefreshToken();
            user.RefreshTokens!.Add(refreshToken);
            await _userManager.UpdateAsync(user);
            return (refreshToken.Token, refreshToken.ExpiresOn);
        }
    }
}
