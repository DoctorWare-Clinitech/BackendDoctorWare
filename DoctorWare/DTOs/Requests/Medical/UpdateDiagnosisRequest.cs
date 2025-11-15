using System;

namespace DoctorWare.DTOs.Requests.Medical
{
    public class UpdateDiagnosisRequest
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Severity { get; set; }
        public DateTime? DiagnosisDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}

