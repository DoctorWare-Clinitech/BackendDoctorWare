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

            result.Select(r => r.Id).Should().ContainInOrder("3", "1", "2");
        }

        [Fact]
        public async Task CancelAppointmentAsync_WhenBelongsToPatient_ReturnsTrue()
        {
            PatientPortalService service = CreateService(out var identityMock, out var appointmentsMock, out var historyMock);
            identityMock.Setup(s => s.GetPatientIdByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(42);
            appointmentsMock.Setup(a => a.GetByIdAsync("abc", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppointmentDto { Id = "abc", PatientId = "42" });
            appointmentsMock.Setup(a => a.CancelAsync("abc", 1, "motivo", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            bool result = await service.CancelAppointmentAsync(1, "abc", "motivo", CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetHistoryAsync_ReturnsEntriesDescending()
        {
            PatientPortalService service = CreateService(out var identityMock, out var appointmentsMock, out var historyMock);
            identityMock.Setup(s => s.GetPatientIdByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(42);
            historyMock.Setup(h => h.GetByPatientAsync("42", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MedicalHistoryDto>
                {
                    new() { Id = "a", Date = new DateTime(2025,5,1) },
                    new() { Id = "b", Date = new DateTime(2025,6,1) }
                });

            IEnumerable<MedicalHistoryDto> result = await service.GetHistoryAsync(1, CancellationToken.None);

            result.Select(r => r.Id).Should().ContainInOrder("b", "a");
        }
    }
}
