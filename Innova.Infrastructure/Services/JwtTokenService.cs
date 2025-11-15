namespace Innova.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IIdentityService _identityService;
        private readonly UserManager<AppUser> _userManager;
        private readonly JwtSettings _jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public JwtTokenService(IIdentityService identityService, UserManager<AppUser> userManager, IOptions<JwtSettings> jwt, IHttpContextAccessor httpContextAccessor)
        {
            _identityService = identityService;
            _userManager = userManager;
            _jwt = jwt.Value;
            _httpContextAccessor = httpContextAccessor;

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
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
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

        public async Task<(string, DateTime)?> GetActiveRefreshToken(string email)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email);

            // check if user has a refresh token
            var activeRefreshToken = user?.RefreshTokens
                ?.FirstOrDefault(rt => rt.IsActive && rt.RevokedOn == null);

            if (activeRefreshToken is not null)
                return (activeRefreshToken.Token, activeRefreshToken.ExpiresOn);
            // at this point, the user has no active refresh token
            var newRefreshToken = GenerateRefreshToken();
            // save the new refresh token
            user?.RefreshTokens?.Add(newRefreshToken);
            await _userManager.UpdateAsync(user!);
            return (newRefreshToken.Token, newRefreshToken.ExpiresOn);
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            return await _userManager.Users
                 .AnyAsync(u => u.RefreshTokens
                     .Any(t => t.Token == refreshToken &&
                               t.RevokedOn == null &&
                               t.ExpiresOn > DateTime.UtcNow));
        }

        public async Task<string> GetUserUserNameFromRefreshTokenAsync(string token)
        {
            var userName = await _userManager.Users
                 .Where(u => u.RefreshTokens!.Any(x => x.Token == token && x.ExpiresOn > DateTime.UtcNow && x.RevokedOn == null))
                 .Select(u => u.UserName)
                 .FirstOrDefaultAsync();
            return userName!;
        }

        public async Task<DateTime> GetRefreshTokenExpirationDate(string token)
        {
            var refreshToken = await _userManager.Users
             .SelectMany(u => u.RefreshTokens)
             .Where(rt => rt.Token == token && rt.RevokedOn == null && rt.ExpiresOn > DateTime.UtcNow)
             .Select(rt => new { rt.ExpiresOn })
             .FirstOrDefaultAsync();

            return refreshToken!.ExpiresOn;
        }

        public void SetTokenCookieAsHttpOnly(string cookieName, string token, DateTime expirationDate)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expirationDate.ToLocalTime(),
                Secure = true,
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, token, cookieOptions);
        }
    }
}
