using System;
using System.Collections.Generic;

namespace DoctorWare.DTOs.Response.Frontend
{
    public class ScheduleTimeSlotDto
    {
        public string Id { get; set; } = string.Empty;

        public int DayOfWeek { get; set; }

        public string StartTime { get; set; } = string.Empty; // HH:mm

        public string EndTime { get; set; } = string.Empty;   // HH:mm

        public int Duration { get; set; }

        public bool IsActive { get; set; }
    }

    public class ScheduleBlockedSlotDto
    {
        public string Id { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string StartTime { get; set; } = string.Empty; // HH:mm

        public string EndTime { get; set; } = string.Empty;   // HH:mm

        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class ScheduleConfigDto
    {
        public string ProfessionalId { get; set; } = string.Empty;

        public List<ScheduleTimeSlotDto> TimeSlots { get; set; } = new List<ScheduleTimeSlotDto>();

        public List<ScheduleBlockedSlotDto> BlockedSlots { get; set; } = new List<ScheduleBlockedSlotDto>();

        public int ConsultationDuration { get; set; }
    }

    public class ScheduleAvailableSlotDto
    {
        public DateTime Date { get; set; }

        public string Time { get; set; } = string.Empty; // HH:mm

        public bool Available { get; set; }

        public string? AppointmentId { get; set; }

        public int Duration { get; set; }
    }
}
