namespace Innova.Domain.Interfaces
{
    public interface IJwtTokenService
    {
        Task<string> CreateTokenAsync(string userName);
        Task<(string, DateTime)?> CreateRefreshTokenAsync(string userName);
    }
}
