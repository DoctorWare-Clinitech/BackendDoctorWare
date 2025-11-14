using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Response;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DoctorWare.Services.Implementation
{
    public class SpecialtyService : DoctorWare.Services.Interfaces.ISpecialtyService
    {
        private readonly IDbConnectionFactory factory;

        public SpecialtyService(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task<List<SpecialtyDto>> GetActiveSpecialtiesAsync(CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            // La tabla ESPECIALIDADES no tiene campo ACTIVO en el modelo; devolvemos todas
            const string sql = "select \"ID_ESPECIALIDADES\" as Id, \"NOMBRE\" as Nombre from public.\"ESPECIALIDADES\" order by \"NOMBRE\"";
            IEnumerable<SpecialtyDto> queryResult = await con.QueryAsync<SpecialtyDto>(sql);
            List<SpecialtyDto> list = queryResult.ToList();
            return list;
        }
    }
}
