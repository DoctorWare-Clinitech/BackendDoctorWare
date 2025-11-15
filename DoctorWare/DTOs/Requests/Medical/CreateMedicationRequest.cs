using System;

namespace DoctorWare.DTOs.Requests.Medical
{
    public class CreateMedicationRequest
    {
        public string PatientId { get; set; } = string.Empty;
        public string? AppointmentId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Instructions { get; set; }
    }
}

