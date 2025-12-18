using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace L.R._Gcaleka__Co.Models
{
    public class EmailServices : IEmailSender
    {
        private readonly ILogger<EmailServices> _logger;
        private readonly SmtpSettings _smtpSettings; //injecting this class to be able to use its properties 
        public EmailServices(SmtpSettings smtpSettings, ILogger<EmailServices> logger)
        {
            _smtpSettings = smtpSettings;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                using (var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port))
                {
                    client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
                    client.EnableSsl = _smtpSettings.EnableSSL;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_smtpSettings.Username),
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Email sent successfully to {Email}", email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending email: {Message}", ex.Message);
            }
        }

        public void SendEmail()
        {
            Console.WriteLine($"Sending email via {_smtpSettings.Server}:{_smtpSettings.Port}");
        }
    }
}
