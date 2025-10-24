namespace DoctorWare.DTOs.Response
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public UserDto User { get; set; } = new UserDto();

        public int ExpiresIn { get; set; }
    }
}
