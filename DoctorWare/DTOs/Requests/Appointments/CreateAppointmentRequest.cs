using System;

namespace DoctorWare.DTOs.Requests.Appointments
{
    public class CreateAppointmentRequest
    {
        public string PatientId { get; set; } = string.Empty; // ID_PACIENTES (string para compatibilidad)
        public string ProfessionalId { get; set; } = string.Empty; // USUARIOS.ID_USUARIOS (string) → se resuelve a PROFESIONALES
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty; // HH:mm
        public int Duration { get; set; }
        public string Type { get; set; } = "first_visit";
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }
}

