using System.Security.Claims;

namespace Innova.Domain.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UserNameExistsAsync(string userName);
        Task<List<string>> CreateUserAsync(string firstName, string lastName, string email, DateTime? dateOfBirth, string userName, string password);
        Task<List<string>> AddToRoleAsync(string userName, string roleName);
        Task<IList<string>> GetRolesAsync(string userName);
        Task<IList<Claim>> GetClaimsAsync(string userName);
        Task<IList<Claim>> GetAllUserClaimsAsync(string userName);
        Task<bool> ValidateUserCredentialsAsync(string email, string password);
        Task<string> GetUserNameByEmailAsync(string email);
    }
}
