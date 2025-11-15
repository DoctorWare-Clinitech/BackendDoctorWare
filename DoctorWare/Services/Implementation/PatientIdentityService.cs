using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.Services.Interfaces;

namespace DoctorWare.Services.Implementation
{
    public class PatientIdentityService : IPatientIdentityService
    {
        private readonly IDbConnectionFactory factory;

        public PatientIdentityService(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task<int?> GetPatientIdByUserIdAsync(int userId, CancellationToken ct)
        {
            using System.Data.IDbConnection con = factory.CreateConnection();
            const string sql = @"
                select pa.""ID_PACIENTES""
                from public.""PACIENTES"" pa
                join public.""PERSONAS"" per on per.""ID_PERSONAS"" = pa.""ID_PERSONAS""
                join public.""USUARIOS"" u on u.""ID_PERSONAS"" = per.""ID_PERSONAS""
                where u.""ID_USUARIOS"" = @uid
                limit 1;";

            int? id = await con.ExecuteScalarAsync<int?>(sql, new { uid = userId });
            return id;
        }
    }
}
