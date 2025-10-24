using System.Threading;
using System.Threading.Tasks;

namespace DoctorWare.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}

