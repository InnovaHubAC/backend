namespace Innova.Domain.Interfaces
{
    public interface IJwtTokenService
    {
        Task<string> CreateTokenAsync(string userName);
        Task<(string, DateTime)?> CreateRefreshTokenAsync(string userName);
        Task<(string, DateTime)?> GetActiveRefreshToken(string email);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);
        Task<string> GetUserUserNameFromRefreshTokenAsync(string token);
        Task<DateTime> GetRefreshTokenExpirationDate(string token);
    }
}
