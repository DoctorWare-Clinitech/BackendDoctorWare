using System;

namespace DoctorWare.DTOs.Response.Medical
{
    public class AllergyDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string Allergen { get; set; } = string.Empty;
        public string Type { get; set; } = "other"; // medication | food | environmental | other
        public string Severity { get; set; } = "low";
        public string? Symptoms { get; set; }
        public DateTime? DiagnosedDate { get; set; }
        public string? Notes { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

