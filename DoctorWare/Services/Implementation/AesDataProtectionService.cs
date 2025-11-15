using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DoctorWare.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DoctorWare.Services.Implementation
{
    public class AesDataProtectionService : IDataProtectionService
    {
        private readonly byte[] keyBytes;
        private readonly ILogger<AesDataProtectionService> logger;
        private const string Prefix = "ENC::";

        public AesDataProtectionService(IConfiguration configuration, ILogger<AesDataProtectionService> logger)
        {
            this.logger = logger;
            string? key = configuration["DataProtection:Key"];
            if (string.IsNullOrWhiteSpace(key))
            {
                key = "9F1C7A7D0C5E43B8A1D79F1C7A7D0C5E"; // fallback dev key
            }

            keyBytes = NormalizeKey(key);
        }

        public string? Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            using Aes aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] plain = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plain, 0, plain.Length);

            string iv = Convert.ToBase64String(aes.IV);
            string cipher = Convert.ToBase64String(cipherBytes);
            return $"{Prefix}{iv}::{cipher}";
        }

        public string? Decrypt(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            if (!cipherText.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return cipherText;
            }

            try
            {
                string payload = cipherText.Substring(Prefix.Length);
                string[] parts = payload.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    return cipherText;
                }

                byte[] iv = Convert.FromBase64String(parts[0]);
                byte[] cipherBytes = Convert.FromBase64String(parts[1]);

                using Aes aes = Aes.Create();
                aes.Key = keyBytes;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using ICryptoTransform decryptor = aes.CreateDecryptor();
                byte[] plain = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(plain);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo desencriptar el valor solicitado.");
                return cipherText;
            }
        }

        private static byte[] NormalizeKey(string key)
        {
            byte[] raw;
            if (key.Length == 32 && key.All(c => Uri.IsHexDigit(c)))
            {
                raw = Enumerable.Range(0, key.Length / 2)
                    .Select(i => Convert.ToByte(key.Substring(i * 2, 2), 16))
                    .ToArray();
            }
            else
            {
                raw = Encoding.UTF8.GetBytes(key);
            }

            if (raw.Length == 32)
            {
                return raw;
            }

            byte[] normalized = new byte[32];
            Array.Copy(raw, normalized, Math.Min(raw.Length, normalized.Length));
            return normalized;
        }
    }
}
