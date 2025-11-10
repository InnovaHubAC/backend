namespace Innova.Domain.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task<bool> SendEmailConfirmationAsync(string toEmail, string userName, string confirmationToken);
    }
}
