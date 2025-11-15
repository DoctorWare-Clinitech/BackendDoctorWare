using System;

namespace DoctorWare.DTOs.Requests.Patients
{
    public class UpdatePatientRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public AddressRequest? Address { get; set; }
        public EmergencyContactRequest? EmergencyContact { get; set; }
        public MedicalInsuranceRequest? MedicalInsurance { get; set; }
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }
    }
}

