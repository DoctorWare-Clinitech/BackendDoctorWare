using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DoctorWare.DTOs.Response.Appointments;
using DoctorWare.DTOs.Response.Medical;
using DoctorWare.Services.Implementation;
using DoctorWare.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace UnitTestDoctorWare.Services
{
    public class PatientPortalServiceTests
    {
        private PatientPortalService CreateService(
            out Mock<IPatientIdentityService> identityMock,
            out Mock<IAppointmentsService> appointmentsMock,
            out Mock<IMedicalHistoryService> historyMock)
        {
            identityMock = new Mock<IPatientIdentityService>(MockBehavior.Strict);
            appointmentsMock = new Mock<IAppointmentsService>(MockBehavior.Strict);
            historyMock = new Mock<IMedicalHistoryService>(MockBehavior.Strict);

            return new PatientPortalService(
                identityMock.Object,
                appointmentsMock.Object,
                historyMock.Object,
                NullLogger<PatientPortalService>.Instance);
        }

        [Fact]
        public async Task GetAppointmentsAsync_ReturnsOrderedAppointments()
        {
            Console.WriteLine("--- PRUEBA: GetAppointmentsAsync_ReturnsOrderedAppointments ---");
            Console.WriteLine("QUÉ SE PROBÓ: El portal de paciente devuelve los turnos ordenados por fecha y hora.");
            PatientPortalService service = CreateService(out var identityMock, out var appointmentsMock, out var historyMock);
            identityMock.Setup(s => s.GetPatientIdByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(42);
            appointmentsMock.Setup(a => a.GetAsync(null, "42", null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AppointmentDto>
                {
                    new() { Id = "2", PatientId = "42", Date = new DateTime(2025,1,10), StartTime = "15:00" },
                    new() { Id = "1", PatientId = "42", Date = new DateTime(2025,1,10), StartTime = "09:00" },
                    new() { Id = "3", PatientId = "42", Date = new DateTime(2024,12,01), StartTime = "08:00" }
                });

            IEnumerable<AppointmentDto> result = await service.GetAppointmentsAsync(1, null, null, CancellationToken.None);

            Console.WriteLine("QUÉ RESULTADO SE ESPERABA: Turnos con IDs en el orden '3', '1', '2'.");
            Console.WriteLine($"QUÉ RESULTADO SE OBTUVO: IDs en el orden '{string.Join(", ", result.Select(r => r.Id))}'.");
            result.Select(r => r.Id).Should().ContainInOrder("3", "1", "2");
            Console.WriteLine("--- RESULTADO: CUMPLIDO ---");
        }

        [Fact]
        public async Task CancelAppointmentAsync_WhenBelongsToPatient_ReturnsTrue()
        {
            Console.WriteLine("--- PRUEBA: CancelAppointmentAsync_WhenBelongsToPatient_ReturnsTrue ---");
            Console.WriteLine("QUÉ SE PROBÓ: Un paciente puede cancelar un turno que le pertenece.");
            PatientPortalService service = CreateService(out var identityMock, out var appointmentsMock, out var historyMock);
            identityMock.Setup(s => s.GetPatientIdByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(42);
            appointmentsMock.Setup(a => a.GetByIdAsync("abc", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppointmentDto { Id = "abc", PatientId = "42" });
            appointmentsMock.Setup(a => a.CancelAsync("abc", 1, "motivo", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            bool result = await service.CancelAppointmentAsync(1, "abc", "motivo", CancellationToken.None);

            Console.WriteLine("QUÉ RESULTADO SE ESPERABA: true");
            Console.WriteLine($"QUÉ RESULTADO SE OBTUVO: {result}");
            result.Should().BeTrue();
            Console.WriteLine("--- RESULTADO: CUMPLIDO ---");
        }

        [Fact]
        public async Task CancelAppointmentAsync_WhenDoesNotBelongToPatient_ThrowsUnauthorizedAccessException()
        {
            Console.WriteLine("--- PRUEBA: CancelAppointmentAsync_WhenDoesNotBelongToPatient_ThrowsUnauthorizedAccessException ---");
            Console.WriteLine("QUÉ SE PROBÓ: Un paciente no puede cancelar un turno que no le pertenece.");
            PatientPortalService service = CreateService(out var identityMock, out var appointmentsMock, out var historyMock);
            identityMock.Setup(s => s.GetPatientIdByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(42); // Patient ID is 42
            appointmentsMock.Setup(a => a.GetByIdAsync("abc", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppointmentDto { Id = "abc", PatientId = "99" }); // Appointment belongs to patient 99

            Func<Task> act = () => service.CancelAppointmentAsync(1, "abc", "motivo", CancellationToken.None);

            Console.WriteLine("QUÉ RESULTADO SE ESPERABA: Una excepción de tipo UnauthorizedAccessException.");
            try
            {
                await act.Should().ThrowAsync<UnauthorizedAccessException>();
                Console.WriteLine("QUÉ RESULTADO SE OBTUVO: Se lanzó UnauthorizedAccessException.");
                Console.WriteLine("--- RESULTADO: CUMPLIDO ---");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"QUÉ RESULTADO SE OBTUVO: Se lanzó una excepción inesperada: {ex.GetType().Name}");
                Console.WriteLine("--- RESULTADO: NO CUMPLIDO ---");
                throw;
            }
        }

        [Fact]
        public async Task GetHistoryAsync_ReturnsEntriesDescending()
        {
            Console.WriteLine("--- PRUEBA: GetHistoryAsync_ReturnsEntriesDescending ---");
            Console.WriteLine("QUÉ SE PROBÓ: El historial clínico del paciente se devuelve en orden cronológico descendente.");
            PatientPortalService service = CreateService(out var identityMock, out var appointmentsMock, out var historyMock);
            identityMock.Setup(s => s.GetPatientIdByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(42);
            historyMock.Setup(h => h.GetByPatientAsync("42", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MedicalHistoryDto>
                {
                    new() { Id = "a", Date = new DateTime(2025,5,1) },
                    new() { Id = "b", Date = new DateTime(2025,6,1) }
                });

            IEnumerable<MedicalHistoryDto> result = await service.GetHistoryAsync(1, CancellationToken.None);

            Console.WriteLine("QUÉ RESULTADO SE ESPERABA: Entradas con IDs en el orden 'b', 'a'.");
            Console.WriteLine($"QUÉ RESULTADO SE OBTUVO: IDs en el orden '{string.Join(", ", result.Select(r => r.Id))}'.");
            result.Select(r => r.Id).Should().ContainInOrder("b", "a");
            Console.WriteLine("--- RESULTADO: CUMPLIDO ---");
        }
    }
}
