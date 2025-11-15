using System.Collections.Generic;

namespace DoctorWare.DTOs.Requests.Medical
{
    public class UpdateMedicalHistoryRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public string? Observations { get; set; }
        public List<string>? Attachments { get; set; }
    }
}

