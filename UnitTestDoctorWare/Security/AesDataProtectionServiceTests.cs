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
            Console.WriteLine("--- PRUEBA: EncryptDecrypt_RoundTrip_PreservesPlainText ---");
            Console.WriteLine("QUÉ SE PROBÓ: El cifrado y descifrado de un texto lo devuelve a su estado original.");
            AesDataProtectionService service = CreateService();

            string? cipher = service.Encrypt("sensitiva");
            cipher.Should().NotBeNull().And.StartWith("ENC::");

            string? plain = service.Decrypt(cipher);

            Console.WriteLine("QUÉ RESULTADO SE ESPERABA: 'sensitiva'");
            Console.WriteLine($"QUÉ RESULTADO SE OBTUVO: '{plain}'");
            plain.Should().Be("sensitiva");
            Console.WriteLine("--- RESULTADO: CUMPLIDO ---");
        }

        [Fact]
        public void Encrypt_NullOrEmpty_ReturnsSame()
        {
            Console.WriteLine("--- PRUEBA: Encrypt_NullOrEmpty_ReturnsSame ---");
            Console.WriteLine("QUÉ SE PROBÓ: Cifrar un valor nulo o vacío devuelve el mismo valor sin cambios.");
            AesDataProtectionService service = CreateService();

            var nullResult = service.Encrypt(null);
            var emptyResult = service.Encrypt(string.Empty);

            Console.WriteLine("QUÉ RESULTADO SE ESPERABA: null y string vacío");
            Console.WriteLine($"QUÉ RESULTADO SE OBTUVO: {(nullResult == null ? "null" : nullResult)} y '{(emptyResult == string.Empty ? "" : "no-vacío")}'");
            nullResult.Should().BeNull();
            emptyResult.Should().Be(string.Empty);
            Console.WriteLine("--- RESULTADO: CUMPLIDO ---");
        }

        [Fact]
        public void Decrypt_InvalidPayload_ReturnsOriginal()
        {
            Console.WriteLine("--- PRUEBA: Decrypt_InvalidPayload_ReturnsOriginal ---");
            Console.WriteLine("QUÉ SE PROBÓ: Descifrar un texto que no es un cifrado válido devuelve el texto original.");
            AesDataProtectionService service = CreateService();

            string? result = service.Decrypt("INVALID");

            Console.WriteLine("QUÉ RESULTADO SE ESPERABA: 'INVALID'");
            Console.WriteLine($"QUÉ RESULTADO SE OBTUVO: '{result}'");
            result.Should().Be("INVALID");
            Console.WriteLine("--- RESULTADO: CUMPLIDO ---");
        }
    }
}
