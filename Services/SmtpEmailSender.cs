 using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace KickFive.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPass"];

            Console.WriteLine($"[SMTP DEBUG] Host={smtpHost}, Port={smtpPort}, User={smtpUser}");

            try
            {
                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.EnableSsl = true;
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("noreply@kickfive.com"),
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(email);
                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine("[SMTP DEBUG] Email sent successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMTP DEBUG] FAILED: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
        }
    }
}
