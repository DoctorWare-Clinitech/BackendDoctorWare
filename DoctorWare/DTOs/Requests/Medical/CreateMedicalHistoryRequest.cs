using System;
using System.Collections.Generic;

namespace DoctorWare.DTOs.Requests.Medical
{
    public class CreateMedicalHistoryRequest
    {
        public string PatientId { get; set; } = string.Empty;
        public string? AppointmentId { get; set; }
        public string Type { get; set; } = "consultation";
        public DateTime? Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public string? Observations { get; set; }
        public List<string>? Attachments { get; set; }
    }
}
