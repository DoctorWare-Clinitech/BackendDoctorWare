using System;
using System.Collections.Generic;

namespace DoctorWare.DTOs.Response.Medical
{
    public class MedicalHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string? AppointmentId { get; set; }
        public string Type { get; set; } = "consultation";
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public string? Observations { get; set; }
        public List<string> Attachments { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

