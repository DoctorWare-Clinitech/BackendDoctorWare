using DoctorWare.DTOs.Requests.Schedule;
using DoctorWare.DTOs.Response.Frontend;

namespace DoctorWare.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<ScheduleConfigDto> GetConfigAsync(string professionalUserId, CancellationToken ct);

        Task<ScheduleConfigDto> UpdateConfigAsync(string professionalUserId, UpdateScheduleConfigRequest request, CancellationToken ct);

        Task<ScheduleTimeSlotDto> AddTimeSlotAsync(string professionalUserId, CreateTimeSlotRequest request, CancellationToken ct);

        Task<ScheduleTimeSlotDto> UpdateTimeSlotAsync(string professionalUserId, string slotId, UpdateTimeSlotRequest request, CancellationToken ct);

        Task DeleteTimeSlotAsync(string professionalUserId, string slotId, CancellationToken ct);

        Task<ScheduleBlockedSlotDto> AddBlockedSlotAsync(string professionalUserId, CreateBlockedSlotRequest request, CancellationToken ct);

        Task DeleteBlockedSlotAsync(string professionalUserId, string blockId, CancellationToken ct);

        Task<IEnumerable<ScheduleBlockedSlotDto>> GetBlockedSlotsAsync(string professionalUserId, DateTime startDate, DateTime endDate, CancellationToken ct);

        Task<IEnumerable<ScheduleAvailableSlotDto>> GetAvailableSlotsAsync(string professionalUserId, DateTime date, CancellationToken ct);
    }
}

