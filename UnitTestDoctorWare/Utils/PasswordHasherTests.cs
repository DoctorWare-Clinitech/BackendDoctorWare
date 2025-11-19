using DoctorWare.Utils;
using FluentAssertions;
using Xunit;

namespace UnitTestDoctorWare.Utils
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashAndVerify_WithSamePassword_ReturnsTrue()
        {
            Console.WriteLine("--- PRUEBA: HashAndVerify_WithSamePassword_ReturnsTrue ---");
            Console.WriteLine("QUÉ SE PROBÓ: Verificar que una contraseña coincida con su propio hash.");
            
            string password = "DoctorWare#2025";
            string hash = PasswordHasher.Hash(password);

            bool result = PasswordHasher.Verify(password, hash);

            Console.WriteLine("QUÉ RESULTADO SE ESPERABA: true");
            Console.WriteLine($"QUÉ RESULTADO SE OBTUVO: {result}");
            result.Should().BeTrue();
            Console.WriteLine("--- RESULTADO: CUMPLIDO ---");
        }

        [Fact]
        public void Verify_WithDifferentPassword_ReturnsFalse()
        {
            Console.WriteLine("--- PRUEBA: Verify_WithDifferentPassword_ReturnsFalse ---");
            Console.WriteLine("QUÉ SE PROBÓ: Verificar que una contraseña no coincida con el hash de otra contraseña.");

            string hash = PasswordHasher.Hash("Secret123");

            bool result = PasswordHasher.Verify("OtherPassword", hash);

            Console.WriteLine("QUÉ RESULTADO SE ESPERABA: false");
            Console.WriteLine($"QUÉ RESULTADO SE OBTUVO: {result}");
            result.Should().BeFalse();
            Console.WriteLine("--- RESULTADO: CUMPLIDO ---");
        }
    }
}
