using Dapper;
using DoctorWare.Data.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data;

namespace DoctorWare.Health
{
    /// <summary>
    /// Health check para verificar la conectividad básica con la base de datos.
    /// Ejecuta SELECT 1 y reporta estado Healthy/Unhealthy según el resultado.
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IDbConnectionFactory factory;

        public DatabaseHealthCheck(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using IDbConnection con = factory.CreateConnection();
                var result = await con.ExecuteScalarAsync<int>("SELECT 1");
                return result == 1
                    ? HealthCheckResult.Healthy("DB OK")
                    : HealthCheckResult.Unhealthy("DB respondió distinto a 1");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("DB no disponible", ex);
            }
        }
    }
}

