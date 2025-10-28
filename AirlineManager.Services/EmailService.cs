using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace AirlineManager.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);

        Task SendWelcomeEmailAsync(string email, string firstName, string lastName);

        Task SendPasswordResetEmailAsync(string email, string resetLink);

        Task SendEmailConfirmationAsync(string email, string firstName, string confirmationLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfigurationService _configService;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfigurationService configService, ILogger<EmailService> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpConfig = await _configService.GetCategoryAsync("SMTP");

                var host = smtpConfig.GetValueOrDefault("SMTP_Host");
                var portStr = smtpConfig.GetValueOrDefault("SMTP_Port", "587");
                var username = smtpConfig.GetValueOrDefault("SMTP_Username");
                var password = smtpConfig.GetValueOrDefault("SMTP_Password");
                var fromEmail = smtpConfig.GetValueOrDefault("SMTP_FromEmail");
                var fromName = smtpConfig.GetValueOrDefault("SMTP_FromName");
                var enableSslStr = smtpConfig.GetValueOrDefault("SMTP_EnableSSL", "true");

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogWarning("SMTP configuration is incomplete. Email not sent.");
                    return;
                }

                if (!int.TryParse(portStr, out var port))
                {
                    port = 587;
                }

                if (!bool.TryParse(enableSslStr, out var enableSsl))
                {
                    enableSsl = true;
                }

                using var client = new SmtpClient(host, port);
                client.EnableSsl = enableSsl;

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    client.Credentials = new NetworkCredential(username, password);
                }

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName, string lastName)
        {
            var subject = "Welcome to Airline Manager!";
            var body = $@"
     <html>
     <head>
            <style>
         body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
         .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
       .header {{ background-color: #1b6ec2; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
            .content {{ background-color: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
   .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
          .button {{ display: inline-block; background-color: #1b6ec2; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
        </style>
       </head>
            <body>
         <div class='container'>
             <div class='header'>
       <h1>Welcome to Airline Manager!</h1>
      </div>
            <div class='content'>
                <h2>Hello {firstName} {lastName},</h2>
          <p>Thank you for creating an account with Airline Manager. Your account has been successfully created and is now active.</p>
                   <p>You can now log in and start using our services.</p>
                 <p><strong>Account Details:</strong></p>
   <ul>
   <li><strong>Email:</strong> {email}</li>
        <li><strong>Registration Date:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
         </ul>
          <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
     <p>Best regards,<br>The Airline Manager Team</p>
       </div>
              <div class='footer'>
       <p>&copy; {DateTime.Now.Year} Airline Manager. All rights reserved.</p>
       </div>
  </div>
          </body>
   </html>
  ";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var subject = "Password Reset Request - Airline Manager";
            var body = $@"
           <html>
             <head>
        <style>
     body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
     .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
  .header {{ background-color: #1b6ec2; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
    .content {{ background-color: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
           .button {{ display: inline-block; background-color: #1b6ec2; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
   .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
   </head>
     <body>
        <div class='container'>
       <div class='header'>
     <h1>Password Reset Request</h1>
    </div>
     <div class='content'>
 <h2>Hello,</h2>
             <p>We received a request to reset your password for your Airline Manager account.</p>
   <p>Click the button below to reset your password:</p>
   <p style='text-align: center;'>
     <a href='{resetLink}' class='button'>Reset Password</a>
       </p>
     <p>Or copy and paste this link into your browser:</p>
   <p style='word-break: break-all; background-color: #fff; padding: 10px; border: 1px solid #ddd;'>{resetLink}</p>
      <div class='warning'>
   <strong>⚠️ Security Notice:</strong> This link will expire in 24 hours. If you didn't request a password reset, please ignore this email or contact our support team.
    </div>
   <p>Best regards,<br>The Airline Manager Team</p>
         </div>
     <div class='footer'>
<p>&copy; {DateTime.Now.Year} Airline Manager. All rights reserved.</p>
   </div>
              </div>
   </body>
              </html>
     ";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendEmailConfirmationAsync(string email, string firstName, string confirmationLink)
        {
            var subject = "Confirm Your Email - Airline Manager";
            var body = $@"
    <html>
            <head>
     <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                     .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
         .header {{ background-color: #1b6ec2; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
   .content {{ background-color: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
  .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
              .button {{ display: inline-block; background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 15px; font-weight: bold; }}
             .info {{ background-color: #d1ecf1; border: 1px solid #bee5eb; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        </style>
      </head>
            <body>
            <div class='container'>
        <div class='header'>
            <h1>Welcome to Airline Manager!</h1>
         </div>
           <div class='content'>
              <h2>Hello {firstName},</h2>
      <p>Thank you for registering with Airline Manager. To complete your registration and activate your account, please confirm your email address.</p>
      <p style='text-align: center;'>
 <a href='{confirmationLink}' class='button'>Confirm Email Address</a>
        </p>
    <p>Or copy and paste this link into your browser:</p>
     <p style='word-break: break-all; background-color: #fff; padding: 10px; border: 1px solid #ddd;'>{confirmationLink}</p>
        <div class='info'>
               <strong>ℹ️ Important:</strong> You must confirm your email address within 24 hours to activate your account. If you didn't create an account with Airline Manager, please ignore this email.
               </div>
  <p>Once confirmed, you'll be able to log in and access all features of Airline Manager.</p>
                  <p>Best regards,<br>The Airline Manager Team</p>
        </div>
          <div class='footer'>
          <p>&copy; {DateTime.Now.Year} Airline Manager. All rights reserved.</p>
      </div>
           </div>
</body>
         </html>
       ";

            await SendEmailAsync(email, subject, body, true);
        }
    }
}