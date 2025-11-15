using System;

namespace DoctorWare.DTOs.Requests.Patients
{
    public class CreatePatientRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "prefer_not_to_say";
        public AddressRequest Address { get; set; } = new();
        public EmergencyContactRequest EmergencyContact { get; set; } = new();
        public MedicalInsuranceRequest? MedicalInsurance { get; set; }
        public string ProfessionalId { get; set; } = string.Empty; // userId del profesional
        public string? Notes { get; set; }
    }

    public class AddressRequest
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class EmergencyContactRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class MedicalInsuranceRequest
    {
        public string Provider { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public DateTime? ValidUntil { get; set; }
    }
}

