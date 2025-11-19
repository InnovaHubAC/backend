namespace Innova.Domain.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task<bool> SendRegisterationEmailConfirmationAsync(string toEmail, string userName, string confirmationToken);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken);
    }
}
