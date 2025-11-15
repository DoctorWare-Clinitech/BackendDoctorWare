using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.Services.Interfaces;
using System.Data;
using System.Linq;
using System;

namespace DoctorWare.Services.Implementation
{
    public class ProfessionalsService : IProfessionalsService
    {
        private readonly IDbConnectionFactory factory;

        public ProfessionalsService(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task<IEnumerable<ProfessionalListItemDto>> GetAsync(int? specialtyId, string? name, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            List<string> filters = new List<string>();
            DynamicParameters p = new DynamicParameters();

            if (specialtyId.HasValue)
            {
                filters.Add("pe.\"ID_ESPECIALIDADES\" = @sid");
                p.Add("sid", specialtyId.Value);
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                filters.Add("(upper(per.\"NOMBRE\") like upper(@q) or upper(per.\"APELLIDO\") like upper(@q))");
                p.Add("q", $"%{name}%");
            }

            string where = filters.Count > 0 ? (" where " + string.Join(" and ", filters)) : string.Empty;

            string sql = $@"
                select distinct
                    prof.""ID_PROFESIONALES"" as IdProfesional,
                    u.""ID_USUARIOS"" as IdUsuario,
                    per.""NOMBRE"" || ' ' || per.""APELLIDO"" as Nombre,
                    prof.""MATRICULA_NACIONAL"" as MatNac,
                    prof.""MATRICULA_PROVINCIAL"" as MatProv,
                    esp.""NOMBRE"" as Especialidad,
                    prof.""ACTIVO"" as Activo
                from public.""PROFESIONALES"" prof
                join public.""PERSONAS"" per on per.""ID_PERSONAS"" = prof.""ID_PERSONAS""
                left join public.""USUARIOS"" u on u.""ID_PERSONAS"" = prof.""ID_PERSONAS""
                left join public.""PROFESIONAL_ESPECIALIDADES"" pe on pe.""ID_PROFESIONALES"" = prof.""ID_PROFESIONALES""
                left join public.""ESPECIALIDADES"" esp on esp.""ID_ESPECIALIDADES"" = pe.""ID_ESPECIALIDADES""{where}
                order by per.""APELLIDO"", per.""NOMBRE""";

            IEnumerable<dynamic> rows = await con.QueryAsync(sql, p);
            return rows.Select(r => new ProfessionalListItemDto
            {
                Id = (int)r.IdProfesional,
                UserId = Convert.ToString(r.IdUsuario ?? r.IdProfesional),
                Name = (string)r.Nombre,
                MatriculaNacional = r.MatNac as string,
                MatriculaProvincial = r.MatProv as string,
                Especialidad = r.Especialidad as string,
                Activo = (bool)r.Activo
            });
        }

        public async Task<ProfessionalListItemDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            IEnumerable<ProfessionalListItemDto> list = await GetAsync(null, null, ct);
            return list.FirstOrDefault(x => x.Id == id);
        }
    }
}
