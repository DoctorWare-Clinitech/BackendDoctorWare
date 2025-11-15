using System.Threading;
using System.Threading.Tasks;

namespace DoctorWare.Services.Interfaces
{
    public interface IAppointmentReminderService
    {
        /// <summary>
        /// Envía recordatorios de turnos pendientes (no enviados) que ocurran dentro
        /// de la ventana configurada (por defecto, próximas 24 horas).
        /// Devuelve la cantidad de recordatorios enviados.
        /// </summary>
        Task<int> SendPendingRemindersAsync(CancellationToken cancellationToken = default);
    }
}

