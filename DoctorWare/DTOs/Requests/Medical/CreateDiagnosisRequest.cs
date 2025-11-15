using System;

namespace DoctorWare.DTOs.Requests.Medical
{
    public class CreateDiagnosisRequest
    {
        public string PatientId { get; set; } = string.Empty;
        public string? AppointmentId { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Severity { get; set; } = "low";
        public DateTime DiagnosisDate { get; set; }
        public string Status { get; set; } = "active";
        public string? Notes { get; set; }
    }
}

