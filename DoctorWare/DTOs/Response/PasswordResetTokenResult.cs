namespace DoctorWare.DTOs.Response
{
    public class PasswordResetTokenResult
    {
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
