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
        Task<string> GenerateEmailConfirmationTokenAsync(string email);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<bool> IsEmailConfirmedAsync(string email);
        Task<bool> UserExistsAsync(string id);
        Task<(string FirstName, string LastName, string UserName)?> GetUserForIdeaAsync(string userId);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<(bool Success, List<string> Errors)> CreateExternalUserAsync(string email, string userName, string firstName, string lastName, string provider, string providerKey);
        Task<string?> GetUserNameByProviderAsync(string provider, string providerKey);
    }
}
