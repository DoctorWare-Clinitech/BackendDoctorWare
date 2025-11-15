using System;

namespace DoctorWare.DTOs.Requests.Medical
{
    public class CreateAllergyRequest
    {
        public string PatientId { get; set; } = string.Empty;
        public string Allergen { get; set; } = string.Empty;
        public string Type { get; set; } = "other";
        public string Severity { get; set; } = "low";
        public string? Symptoms { get; set; }
        public DateTime? DiagnosedDate { get; set; }
        public string? Notes { get; set; }
    }
}

