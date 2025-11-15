using System;
using System.Collections.Generic;

namespace DoctorWare.DTOs.Requests.Schedule
{
    public class UpdateScheduleConfigRequest
    {
        public int? ConsultationDuration { get; set; }

        public List<TimeSlotConfigRequest>? TimeSlots { get; set; }
    }

    public class TimeSlotConfigRequest
    {
        public int DayOfWeek { get; set; }

        public string StartTime { get; set; } = string.Empty; // HH:mm

        public string EndTime { get; set; } = string.Empty;   // HH:mm

        public int Duration { get; set; }

        public bool IsActive { get; set; }
    }
}

