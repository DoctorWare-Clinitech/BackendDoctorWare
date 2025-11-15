using System;

namespace DoctorWare.DTOs.Requests.Medical
{
    public class UpdateMedicationRequest
    {
        public string? MedicationName { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Instructions { get; set; }
        public bool? Active { get; set; }
    }
}

