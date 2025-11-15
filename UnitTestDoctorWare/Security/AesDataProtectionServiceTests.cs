using System.Collections.Generic;
using DoctorWare.Services.Implementation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UnitTestDoctorWare.Security
{
    public class AesDataProtectionServiceTests
    {
        private static AesDataProtectionService CreateService()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["DataProtection:Key"] = "0123456789ABCDEF0123456789ABCDEF"
                })
                .Build();
            return new AesDataProtectionService(config, NullLogger<AesDataProtectionService>.Instance);
        }

        [Fact]
        public void EncryptDecrypt_RoundTrip_PreservesPlainText()
        {
            AesDataProtectionService service = CreateService();

            string? cipher = service.Encrypt("sensitiva");
            cipher.Should().NotBeNull().And.StartWith("ENC::");

            string? plain = service.Decrypt(cipher);
            plain.Should().Be("sensitiva");
        }

        [Fact]
        public void Encrypt_NullOrEmpty_ReturnsSame()
        {
            AesDataProtectionService service = CreateService();

            service.Encrypt(null).Should().BeNull();
            service.Encrypt(string.Empty).Should().Be(string.Empty);
        }

        [Fact]
        public void Decrypt_InvalidPayload_ReturnsOriginal()
        {
            AesDataProtectionService service = CreateService();

            string? result = service.Decrypt("INVALID");

            result.Should().Be("INVALID");
        }
    }
}
