using System;
using System.Collections.Generic;

namespace DoctorWare.DTOs.Response.Patients
{
    public class PatientDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public int Age => DateOfBirth.HasValue ? (int)Math.Floor((DateTime.UtcNow - DateOfBirth.Value).TotalDays / 365.25) : 0;
        public string Gender { get; set; } = "prefer_not_to_say";
        public AddressDto Address { get; set; } = new();
        public EmergencyContactDto EmergencyContact { get; set; } = new();
        public MedicalInsuranceDto? MedicalInsurance { get; set; }
        public MedicalInfoDto MedicalInfo { get; set; } = new();
        public string ProfessionalId { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AddressDto
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class EmergencyContactDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class MedicalInsuranceDto
    {
        public string Provider { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public DateTime? ValidUntil { get; set; }
    }

    public class MedicalInfoDto
    {
        public string? BloodType { get; set; }
        public List<string> Allergies { get; set; } = new();
        public List<string> ChronicConditions { get; set; } = new();
        public List<string> CurrentMedications { get; set; } = new();
        public List<string> Surgeries { get; set; } = new();
        public string? FamilyHistory { get; set; }
    }

    public class PatientSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Dni { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? LastAppointment { get; set; }
        public DateTime? NextAppointment { get; set; }
        public int TotalAppointments { get; set; }
        public int ActiveConditions { get; set; }
    }
}

