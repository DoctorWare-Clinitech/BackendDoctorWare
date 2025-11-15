using System;

namespace DoctorWare.DTOs.Requests.Schedule
{
    public class CreateTimeSlotRequest
    {
        public int DayOfWeek { get; set; }

        public string StartTime { get; set; } = string.Empty; // HH:mm

        public string EndTime { get; set; } = string.Empty;   // HH:mm

        public int Duration { get; set; }

        public bool IsActive { get; set; }
    }

    public class UpdateTimeSlotRequest
    {
        public int? DayOfWeek { get; set; }

        public string? StartTime { get; set; } // HH:mm

        public string? EndTime { get; set; }   // HH:mm

        public int? Duration { get; set; }

        public bool? IsActive { get; set; }
    }
}

