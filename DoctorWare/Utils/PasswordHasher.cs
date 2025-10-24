using System.Security.Cryptography;
using System.Text;

namespace DoctorWare.Utils
{
    public static class PasswordHasher
    {
        public static string Hash(string password, string? salt = null)
        {
            salt ??= Guid.NewGuid().ToString("N");
            byte[] combined = Encoding.UTF8.GetBytes(password + ":" + salt);
            byte[] hash = SHA256.HashData(combined);
            return $"{salt}.{Convert.ToHexString(hash)}";
        }

        public static bool Verify(string password, string hashWithSalt)
        {
            string[] parts = hashWithSalt?.Split('.', 2);

            if (parts.Length != 2)
            {
                return false;
            }

            string salt = parts[0];
            string expected = Hash(password, salt);
            return expected.Equals(hashWithSalt, StringComparison.Ordinal);
        }
    }
}

