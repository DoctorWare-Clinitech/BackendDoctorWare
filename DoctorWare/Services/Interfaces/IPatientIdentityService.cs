using System.Threading;
using System.Threading.Tasks;

namespace DoctorWare.Services.Interfaces
{
    public interface IPatientIdentityService
    {
        Task<int?> GetPatientIdByUserIdAsync(int userId, CancellationToken ct);
    }
}
