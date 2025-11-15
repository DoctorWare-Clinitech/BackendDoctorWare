using System;

namespace DoctorWare.DTOs.Response.Medical
{
    public class DiagnosisDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string ProfessionalId { get; set; } = string.Empty;
        public string? AppointmentId { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Severity { get; set; } = "low";
        public DateTime DiagnosisDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string Status { get; set; } = "active"; // active | resolved | chronic
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

