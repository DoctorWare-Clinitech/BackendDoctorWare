using System;

namespace DoctorWare.DTOs.Response.Appointments
{
    public class AppointmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string ProfessionalId { get; set; } = string.Empty; // User ID relacionado
        public string ProfessionalName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty; // HH:mm
        public string EndTime { get; set; } = string.Empty;   // HH:mm
        public int Duration { get; set; }
        public string Status { get; set; } = string.Empty; // scheduled | confirmed | in_progress | completed | cancelled | no_show
        public string Type { get; set; } = string.Empty;   // first_visit | follow_up | emergency | routine | specialist
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string? Observations { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelledBy { get; set; }
        public string? CancellationReason { get; set; }
    }
}

