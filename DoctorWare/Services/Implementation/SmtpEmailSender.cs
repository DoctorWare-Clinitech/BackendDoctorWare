using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace DoctorWare.Services.Implementation
{
    public class SmtpEmailSender : DoctorWare.Services.Interfaces.IEmailSender
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<SmtpEmailSender> logger;

        public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var smtpSection = configuration.GetSection("Email:Smtp");
            var host = smtpSection["Host"] ?? "localhost";
            var port = int.TryParse(smtpSection["Port"], out var p) ? p : 25;
            var enableSsl = bool.TryParse(smtpSection["EnableSsl"], out var ssl) && ssl;
            var user = smtpSection["User"] ?? string.Empty;
            var password = smtpSection["Password"] ?? string.Empty;
            var fromEmail = smtpSection["FromEmail"] ?? "no-reply@localhost";
            var fromName = smtpSection["FromName"] ?? "DoctorWare";

            using var message = new MailMessage();
            message.To.Add(new MailAddress(toEmail));
            message.From = new MailAddress(fromEmail, fromName);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = htmlBody;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = string.IsNullOrWhiteSpace(user) && string.IsNullOrWhiteSpace(password),
                Credentials = string.IsNullOrWhiteSpace(user) ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(user, password)
            };

            try
            {
                await client.SendMailAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error enviando email a {To}", toEmail);
                throw;
            }
        }
    }
}

