using System;

namespace DoctorWare.DTOs.Requests.Medical
{
    public class UpdateAllergyRequest
    {
        public string? Type { get; set; }
        public string? Severity { get; set; }
        public string? Symptoms { get; set; }
        public DateTime? DiagnosedDate { get; set; }
        public string? Notes { get; set; }
        public bool? Active { get; set; }
    }
}

