using System;

namespace DoctorWare.DTOs.Requests.Public
{
    public class PublicAppointmentRequest
    {
        public string ProfessionalId { get; set; } = string.Empty; // USUARIOS.ID_USUARIOS del profesional

        public DateTime Date { get; set; }

        public string StartTime { get; set; } = string.Empty; // HH:mm

        public int? Duration { get; set; }

        public string Type { get; set; } = "first_visit";

        public string? Reason { get; set; }

        public string? Notes { get; set; }

        public string? ExistingPatientId { get; set; }

        public string? ExistingPatientUserId { get; set; }

        public PublicPatientInfo Patient { get; set; } = new PublicPatientInfo();
    }

    public class PublicPatientInfo
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Dni { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = "prefer_not_to_say";

        public string? Notes { get; set; }
    }
}
