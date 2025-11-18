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
            Console.WriteLine("--- PRUEBA: Record_ShouldAccumulateStats ---");
            Console.WriteLine("QUÉ SE PROBÓ: El servicio de métricas acumula correctamente las estadísticas de las solicitudes (total, máximo, promedio, etc.).");
            
            RequestMetricsService service = new();

            service.Record("/api/test", 200, 120);
            service.Record("/api/test", 200, 60);
            service.Record("/api/other", 500, 30);

            var snapshot = service.GetSnapshot();

            Console.WriteLine("QUÉ RESULTADO SE ESPERABA: Total=3, MaxMs=120, AvgMs=70, Paths=2");
            Console.WriteLine($"QUÉ RESULTADO SE OBTUVO: Total={snapshot.TotalRequests}, MaxMs={snapshot.MaxMilliseconds}, AvgMs={snapshot.AverageMilliseconds}, Paths={snapshot.RequestsByPath.Count}");

            snapshot.TotalRequests.Should().Be(3);
            snapshot.MaxMilliseconds.Should().Be(120);
            snapshot.AverageMilliseconds.Should().BeApproximately(70d, 0.001);
            snapshot.RequestsByPath.Should().ContainKey("/api/test").WhoseValue.Should().Be(2);
            snapshot.RequestsByPath.Should().ContainKey("/api/other").WhoseValue.Should().Be(1);
            Console.WriteLine("--- RESULTADO: CUMPLIDO ---");
        }
    }
}
