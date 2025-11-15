using System;

namespace DoctorWare.DTOs.Requests.Schedule
{
    public class CreateBlockedSlotRequest
    {
        public DateTime Date { get; set; }

        public string StartTime { get; set; } = string.Empty; // HH:mm

        public string EndTime { get; set; } = string.Empty;   // HH:mm

        public string Reason { get; set; } = string.Empty;

        public string? Type { get; set; }
    }
}

