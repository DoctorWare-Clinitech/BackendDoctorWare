using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Response;
using DoctorWare.Models;
using DoctorWare.Repositories.Interfaces;
using DoctorWare.Services.Implementation;
using DoctorWare.Services.Interfaces;
using DoctorWare.Utils;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace UnitTestDoctorWare.Services
{
    public class UsersServiceTests
    {
        private UsersService CreateService(
            out Mock<IUsuariosRepository> usuariosRepoMock,
            out Mock<IPersonasRepository> personasRepoMock)
        {
            usuariosRepoMock = new Mock<IUsuariosRepository>(MockBehavior.Strict);
            personasRepoMock = new Mock<IPersonasRepository>(MockBehavior.Strict);
            Mock<IDbConnectionFactory> factoryMock = new(MockBehavior.Loose);
            Mock<IEmailConfirmationService> emailConfirmationMock = new(MockBehavior.Loose);

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["PasswordReset:TokenSecret"] = "unit-test-secret",
                    ["PasswordReset:TokenMinutes"] = "60"
                })
                .Build();

            return new UsersService(
                usuariosRepoMock.Object,
                personasRepoMock.Object,
                factoryMock.Object,
                emailConfirmationMock.Object,
                config);
        }

        [Fact]
        public async Task GeneratePasswordResetTokenAsync_UserExists_ReturnsToken()
        {
            UsersService service = CreateService(out Mock<IUsuariosRepository> usuariosRepo, out Mock<IPersonasRepository> personasRepo);
            USUARIOS usuario = new()
            {
                ID_USUARIOS = 10,
                EMAIL = "user@test.com",
                ACTIVO = true,
                TOKEN_RECUPERACION = string.Empty,
                ID_PERSONAS = 5
            };
            usuariosRepo.Setup(r => r.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            usuariosRepo.Setup(r => r.UpdateAsync(It.IsAny<USUARIOS>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            personasRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PERSONAS { NOMBRE = "Ana", APELLIDO = "Pérez" });

            PasswordResetTokenResult? result = await service.GeneratePasswordResetTokenAsync("user@test.com", CancellationToken.None);

            result.Should().NotBeNull();
            result!.Email.Should().Be("user@test.com");
            result.FullName.Should().Be("Ana Pérez");
            result.Token.Should().NotBeNullOrWhiteSpace();
            usuariosRepo.Verify(r => r.UpdateAsync(It.Is<USUARIOS>(u => !string.IsNullOrEmpty(u.TOKEN_RECUPERACION)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GeneratePasswordResetTokenAsync_UserNotFound_ReturnsNull()
        {
            UsersService service = CreateService(out Mock<IUsuariosRepository> usuariosRepo, out _);
            usuariosRepo.Setup(r => r.GetByEmailAsync("unknown@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((USUARIOS?)null);

            PasswordResetTokenResult? result = await service.GeneratePasswordResetTokenAsync("unknown@test.com", CancellationToken.None);

            result.Should().BeNull();
            usuariosRepo.Verify(r => r.UpdateAsync(It.IsAny<USUARIOS>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithValidToken_UpdatesPassword()
        {
            UsersService service = CreateService(out Mock<IUsuariosRepository> usuariosRepo, out _);
            const string token = "abc123";
            string tokenHash = ComputeHash(token, "unit-test-secret");
            USUARIOS usuario = new()
            {
                ID_USUARIOS = 1,
                ACTIVO = true,
                TOKEN_RECUPERACION = tokenHash,
                TOKEN_EXPIRACION = DateTime.UtcNow.AddMinutes(10),
                PASSWORD_HASH = DoctorWare.Utils.PasswordHasher.Hash("OldPass1!")
            };
            usuariosRepo.Setup(r => r.GetByPasswordResetTokenAsync(tokenHash, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            usuariosRepo.Setup(r => r.UpdateAsync(It.IsAny<USUARIOS>(), It.IsAny<CancellationToken>()))
                .Callback<USUARIOS, CancellationToken>((u, _) =>
                {
                    // no-op; verification se hará sobre la instancia original
                })
                .Returns(Task.CompletedTask);
            string previousHash = usuario.PASSWORD_HASH;

            await service.ResetPasswordAsync(token, "NuevaPass123", CancellationToken.None);

            usuario.TOKEN_RECUPERACION.Should().BeEmpty();
            usuario.TOKEN_EXPIRACION.Should().BeNull();
            usuario.PASSWORD_HASH.Should().NotBe(previousHash);
            usuariosRepo.Verify(r => r.UpdateAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_InvalidToken_Throws()
        {
            UsersService service = CreateService(out Mock<IUsuariosRepository> usuariosRepo, out _);
            usuariosRepo.Setup(r => r.GetByPasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((USUARIOS?)null);

            Func<Task> act = () => service.ResetPasswordAsync("bad-token", "NuevaPass123", CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            usuariosRepo.Verify(r => r.UpdateAsync(It.IsAny<USUARIOS>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static string ComputeHash(string token, string secret)
        {
            using SHA256 sha = SHA256.Create();
            byte[] payload = Encoding.UTF8.GetBytes($"{token}|{secret}");
            byte[] hash = sha.ComputeHash(payload);
            return Convert.ToHexString(hash);
        }
    }
}
