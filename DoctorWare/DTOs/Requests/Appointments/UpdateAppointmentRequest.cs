using System;

namespace DoctorWare.DTOs.Requests.Appointments
{
    public class UpdateAppointmentRequest
    {
        public DateTime? Date { get; set; }
        public string? StartTime { get; set; }
        public int? Duration { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string? Observations { get; set; }
    }
}

