using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Requests.Medical;
using DoctorWare.DTOs.Response.Medical;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System;
using System.Linq;

namespace DoctorWare.Services.Implementation
{
    public class AllergiesService : DoctorWare.Services.Interfaces.IAllergiesService
    {
        private readonly IDbConnectionFactory factory;

        public AllergiesService(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task<IEnumerable<AllergyDto>> GetByPatientAsync(string patientId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            const string sql = @"
                select pa.""ID_PACIENTE_ALERGIAS"" as Id,
                       pa.""ID_PACIENTES"" as IdPac,
                       a.""NOMBRE"" as Alergeno,
                       pa.""DETALLES"" as Detalles,
                       pa.""FECHA_CREACION"" as Creado,
                       pa.""ULTIMA_ACTUALIZACION"" as Actualizado
                  from public.""PACIENTE_ALERGIAS"" pa
                  join public.""ALERGIAS"" a on a.""ID_ALERGIAS"" = pa.""ID_ALERGIAS""
                 where pa.""ID_PACIENTES"" = @pid";

            int pid = int.Parse(patientId);
            IEnumerable<dynamic> rows = await con.QueryAsync(sql, new { pid });
            List<AllergyDto> list = rows.Select(MapRow).ToList();
            return list;
        }

        public async Task<AllergyDto> CreateAsync(CreateAllergyRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            int idAlergias = await EnsureAllergen(con, request.Allergen);
            string detalles = JsonConvert.SerializeObject(new
            {
                type = request.Type,
                severity = request.Severity,
                symptoms = request.Symptoms,
                diagnosedDate = request.DiagnosedDate,
                notes = request.Notes,
                active = true
            });

            const string sql = @"
                insert into public.""PACIENTE_ALERGIAS"" (
                    ""ID_PACIENTES"", ""ID_ALERGIAS"", ""DETALLES""
                ) values (
                    @pid, @idA, @det
                ) returning ""ID_PACIENTE_ALERGIAS""";

            int newId = await con.ExecuteScalarAsync<int>(sql, new { pid = int.Parse(request.PatientId), idA = idAlergias, det = detalles });
            IEnumerable<AllergyDto> list = await GetByPatientAsync(request.PatientId, ct);
            AllergyDto created = list.First(x => x.Id == newId.ToString());
            return created;
        }

        public async Task<AllergyDto> UpdateAsync(string id, UpdateAllergyRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            string? json = await con.ExecuteScalarAsync<string>("select \"DETALLES\" from public.\"PACIENTE_ALERGIAS\" where \"ID_PACIENTE_ALERGIAS\" = @id", new { id = int.Parse(id) });
            dynamic meta = string.IsNullOrWhiteSpace(json) ? new { type = "other", severity = "low", symptoms = (string?)null, diagnosedDate = (DateTime?)null, notes = (string?)null, active = true } : JsonConvert.DeserializeObject(json!);
            string newJson = JsonConvert.SerializeObject(new
            {
                type = request.Type ?? (string)meta.type,
                severity = request.Severity ?? (string)meta.severity,
                symptoms = request.Symptoms ?? (string?)meta.symptoms,
                diagnosedDate = request.DiagnosedDate ?? (DateTime?)meta.diagnosedDate,
                notes = request.Notes ?? (string?)meta.notes,
                active = request.Active ?? (bool)meta.active
            });

            const string sql = @"
                update public.""PACIENTE_ALERGIAS""
                   set ""DETALLES"" = @det, ""ULTIMA_ACTUALIZACION"" = NOW()
                 where ""ID_PACIENTE_ALERGIAS"" = @id";
            await con.ExecuteAsync(sql, new { id = int.Parse(id), det = newJson });

            AllergyDto? updated = await GetByIdAsync(con, id);
            if (updated == null)
            {
                throw new InvalidOperationException("Alergia no encontrada luego de actualizar");
            }
            return updated;
        }

        public async Task<bool> DeactivateAsync(string id, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            string? json = await con.ExecuteScalarAsync<string>("select \"DETALLES\" from public.\"PACIENTE_ALERGIAS\" where \"ID_PACIENTE_ALERGIAS\" = @id", new { id = int.Parse(id) });
            dynamic meta = string.IsNullOrWhiteSpace(json) ? new { active = true } : JsonConvert.DeserializeObject(json!);
            string newJson = JsonConvert.SerializeObject(new
            {
                type = (string?)(meta.type ?? "other"),
                severity = (string?)(meta.severity ?? "low"),
                symptoms = (string?)(meta.symptoms ?? null),
                diagnosedDate = (DateTime?)(meta.diagnosedDate ?? null),
                notes = (string?)(meta.notes ?? null),
                active = false
            });
            int affected = await con.ExecuteAsync("update public.\"PACIENTE_ALERGIAS\" set \"DETALLES\" = @det, \"ULTIMA_ACTUALIZACION\" = NOW() where \"ID_PACIENTE_ALERGIAS\" = @id", new { id = int.Parse(id), det = newJson });
            return affected > 0;
        }

        private static AllergyDto MapRow(dynamic r)
        {
            string? detalles = r.Detalles as string;
            string type = "other";
            string severity = "low";
            string? symptoms = null;
            DateTime? diagnosedDate = null;
            string? notes = null;
            bool active = true;
            if (!string.IsNullOrWhiteSpace(detalles))
            {
                try
                {
                    dynamic meta = JsonConvert.DeserializeObject(detalles!);
                    type = meta.type ?? type;
                    severity = meta.severity ?? severity;
                    symptoms = meta.symptoms ?? symptoms;
                    diagnosedDate = meta.diagnosedDate ?? diagnosedDate;
                    notes = meta.notes ?? notes;
                    active = meta.active ?? active;
                }
                catch
                {
                }
            }

            AllergyDto dto = new AllergyDto
            {
                Id = Convert.ToString(r.Id),
                PatientId = Convert.ToString(r.IdPac),
                Allergen = (string)r.Alergeno,
                Type = type,
                Severity = severity,
                Symptoms = symptoms,
                DiagnosedDate = diagnosedDate,
                Notes = notes,
                Active = active,
                CreatedAt = (DateTime)r.Creado,
                UpdatedAt = (DateTime)r.Actualizado
            };
            return dto;
        }

        private static async Task<int> EnsureAllergen(IDbConnection con, string allergen)
        {
            int? id = await con.ExecuteScalarAsync<int?>("select \"ID_ALERGIAS\" from public.\"ALERGIAS\" where upper(\"NOMBRE\") = upper(@n) limit 1", new { n = allergen });
            if (id.HasValue)
            {
                return id.Value;
            }
            int newId = await con.ExecuteScalarAsync<int>("insert into public.\"ALERGIAS\" (\"NOMBRE\") values (@n) returning \"ID_ALERGIAS\"", new { n = allergen });
            return newId;
        }

        private static async Task<AllergyDto?> GetByIdAsync(IDbConnection con, string id)
        {
            const string sql = @"
                select pa.""ID_PACIENTE_ALERGIAS"" as Id,
                       pa.""ID_PACIENTES"" as IdPac,
                       a.""NOMBRE"" as Alergeno,
                       pa.""DETALLES"" as Detalles,
                       pa.""FECHA_CREACION"" as Creado,
                       pa.""ULTIMA_ACTUALIZACION"" as Actualizado
                  from public.""PACIENTE_ALERGIAS"" pa
                  join public.""ALERGIAS"" a on a.""ID_ALERGIAS"" = pa.""ID_ALERGIAS""
                 where pa.""ID_PACIENTE_ALERGIAS"" = @id limit 1";
            dynamic? row = await con.QuerySingleOrDefaultAsync(sql, new { id = int.Parse(id) });
            return row == null ? null : MapRow(row);
        }
    }
}
