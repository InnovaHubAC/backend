namespace Innova.Infrastructure.Services;

public class EmailService : IEmailService
{
  private readonly EmailSettings _emailSettings;

  public EmailService(IOptions<EmailSettings> emailSettings)
  {
    _emailSettings = emailSettings.Value;
  }

  public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
  {
    try
    {
      var message = new MimeMessage();
      message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
      message.To.Add(MailboxAddress.Parse(toEmail));
      message.Subject = subject;

      var bodyBuilder = new BodyBuilder();
      if (isHtml)
      {
        bodyBuilder.HtmlBody = body;
      }
      else
      {
        bodyBuilder.TextBody = body;
      }
      message.Body = bodyBuilder.ToMessageBody();

      using var smtp = new SmtpClient();
      await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port,
          _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
      await smtp.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
      await smtp.SendAsync(message);
      await smtp.DisconnectAsync(true);

      return true;
    }
    catch (Exception ex)
    {
      // TODO: log the exception
      return false;
    }
  }

  public async Task<bool> SendEmailConfirmationAsync(string toEmail, string userName, string confirmationToken)
  {
    var subject = "Confirm Your Email - Innova";

    // this is for safety
    var encodedToken = Uri.EscapeDataString(confirmationToken);
    var encodedEmail = Uri.EscapeDataString(toEmail);

    // TODO: to be changed later
    var frontendURL = "https://yourapp.com/verify-email";
    var confirmationLink = $"{frontendURL}?email={encodedEmail}&token={encodedToken}";

    var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Welcome to Innova, {userName}!</h2>
                        <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' 
                               style='background-color: #007bff; color: white; padding: 12px 30px; 
                                      text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Confirm Email
                            </a>
                        </div>
                        <p style='color: #666; font-size: 14px;'>
                            If the button doesn't work, copy and paste this link into your browser:
                        </p>
                        <p style='color: #007bff; word-break: break-all; font-size: 12px;'>
                            {confirmationLink}
                        </p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                        <p style='color: #999; font-size: 12px;'>
                            If you didn't create an account, please ignore this email.
                        </p>
                    </div>
                </body>
                </html>
            ";

    return await SendEmailAsync(toEmail, subject, body, true);
  }
}

