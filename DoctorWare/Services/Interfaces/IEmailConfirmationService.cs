using DoctorWare.DTOs.Response;
using System.Threading;
using System.Threading.Tasks;

namespace DoctorWare.Services.Interfaces
{
    public interface IEmailConfirmationService
    {
        Task SendConfirmationEmailAsync(int userId, CancellationToken cancellationToken = default, bool bypassCooldown = false);

        Task<EmailConfirmationResult> ConfirmAsync(int userId, string token, CancellationToken cancellationToken = default);

        Task<ResendConfirmationResult> ResendAsync(string email, CancellationToken cancellationToken = default);
    }
}

