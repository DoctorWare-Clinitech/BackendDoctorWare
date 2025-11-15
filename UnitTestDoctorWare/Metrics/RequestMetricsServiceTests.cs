using DoctorWare.Services.Implementation;
using FluentAssertions;
using Xunit;

namespace UnitTestDoctorWare.Metrics
{
    public class RequestMetricsServiceTests
    {
        [Fact]
        public void Record_ShouldAccumulateStats()
        {
            RequestMetricsService service = new();

            service.Record("/api/test", 200, 120);
            service.Record("/api/test", 200, 60);
            service.Record("/api/other", 500, 30);

            var snapshot = service.GetSnapshot();

            snapshot.TotalRequests.Should().Be(3);
            snapshot.MaxMilliseconds.Should().Be(120);
            snapshot.AverageMilliseconds.Should().BeApproximately((120 + 60 + 30) / 3d, 0.001);
            snapshot.RequestsByPath.Should().ContainKey("/api/test").WhoseValue.Should().Be(2);
            snapshot.RequestsByPath.Should().ContainKey("/api/other").WhoseValue.Should().Be(1);
        }
    }
}
