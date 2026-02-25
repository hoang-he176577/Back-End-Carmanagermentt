using Microsoft.Extensions.Configuration;
using Service.Exceptions;
using Service.Services.Auth.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Service.Services.Auth.Implementations
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _config["Email:Host"] ?? _config["Smtp:Host"];
            var username = _config["Email:User"] ?? _config["Smtp:Username"];
            var password = _config["Email:Password"] ?? _config["Smtp:Password"];
            var fromEmail = _config["Email:From"] ?? _config["Smtp:FromEmail"];
            var fromName = _config["Email:FromName"] ?? _config["Smtp:FromName"] ?? "CarManagement";

            var portValue = _config["Email:Port"] ?? _config["Smtp:Port"];
            var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 587;

            var enableSslValue = _config["Email:EnableSsl"] ?? _config["Smtp:EnableSsl"] ?? "true";
            var enableSsl = !bool.TryParse(enableSslValue, out var ssl) || ssl;

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw BusinessErrors.BadRequest("SMTP is not configured. Please set Smtp settings.");
            }

            // Gmail app passwords are often copied with spaces every 4 chars.
            password = Regex.Replace(password, "\\s+", string.Empty);
            username = username.Trim();
            fromEmail = fromEmail.Trim();
            if (string.Equals(password, "your-gmail-app-password", StringComparison.OrdinalIgnoreCase))
            {
                throw BusinessErrors.BadRequest("Email:Password is still a placeholder. Set your real Gmail App Password.");
            }

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(host, port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            try
            {
                await client.SendMailAsync(message);
            }
            catch (SmtpException)
            {
                throw BusinessErrors.BadRequest(
                    "SMTP authentication failed. Check Email:User and Email:Password (Gmail App Password).");
            }
        }
    }
}
