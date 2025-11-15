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
            string password = "DoctorWare#2025";
            string hash = PasswordHasher.Hash(password);

            bool result = PasswordHasher.Verify(password, hash);

            result.Should().BeTrue();
        }

        [Fact]
        public void Verify_WithDifferentPassword_ReturnsFalse()
        {
            string hash = PasswordHasher.Hash("Secret123");

            bool result = PasswordHasher.Verify("OtherPassword", hash);

            result.Should().BeFalse();
        }
    }
}
