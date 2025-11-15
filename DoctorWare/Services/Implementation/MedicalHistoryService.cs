using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Requests.Medical;
using DoctorWare.DTOs.Response.Medical;
using Newtonsoft.Json;
using System.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DoctorWare.Services.Implementation
{
    public class MedicalHistoryService : DoctorWare.Services.Interfaces.IMedicalHistoryService
    {
        private readonly IDbConnectionFactory factory;

        public MedicalHistoryService(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task<IEnumerable<MedicalHistoryDto>> GetByPatientAsync(string patientId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            const string sql = @"
                select h.""ID_HISTORIAS_CLINICAS"" as Id,
                       h.""FECHA"" as Fecha,
                       h.""DIAGNOSTICO"" as Diagnostico,
                       h.""SINTOMA"" as Sintoma,
                       h.""TRATAMIENTO"" as Tratamiento,
                       h.""NOTAS"" as Notas,
                       h.""ADJUNTOS"" as Adjuntos,
                       h.""ID_PACIENTES"" as IdPaciente,
                       h.""ID_PROFESIONALES"" as IdProf,
                       h.""ID_TURNOS"" as IdTurno,
                       h.""FECHA_CREACION"" as Creado,
                       h.""ULTIMA_ACTUALIZACION"" as Actualizado,
                       per.""NOMBRE"" || ' ' || per.""APELLIDO"" as ProfNombre,
                       u.""ID_USUARIOS"" as IdUsuario
                from public.""HISTORIAS_CLINICAS"" h
                join public.""PROFESIONALES"" p on p.""ID_PROFESIONALES"" = h.""ID_PROFESIONALES""
                join public.""PERSONAS"" per on per.""ID_PERSONAS"" = p.""ID_PERSONAS""
                left join public.""USUARIOS"" u on u.""ID_PERSONAS"" = per.""ID_PERSONAS""
                where h.""ID_PACIENTES"" = @id
                order by h.""FECHA"" desc, h.""ID_HISTORIAS_CLINICAS"" desc";

            int pid = int.Parse(patientId);
            IEnumerable<dynamic> rows = await con.QueryAsync(sql, new { id = pid });
            return rows.Select(MapRowToDto);
        }

        public async Task<MedicalHistoryDto?> GetByIdAsync(string id, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            const string sql = @"
                select h.""ID_HISTORIAS_CLINICAS"" as Id,
                       h.""FECHA"" as Fecha,
                       h.""DIAGNOSTICO"" as Diagnostico,
                       h.""SINTOMA"" as Sintoma,
                       h.""TRATAMIENTO"" as Tratamiento,
                       h.""NOTAS"" as Notas,
                       h.""ADJUNTOS"" as Adjuntos,
                       h.""ID_PACIENTES"" as IdPaciente,
                       h.""ID_PROFESIONALES"" as IdProf,
                       h.""ID_TURNOS"" as IdTurno,
                       h.""FECHA_CREACION"" as Creado,
                       h.""ULTIMA_ACTUALIZACION"" as Actualizado,
                       per.""NOMBRE"" || ' ' || per.""APELLIDO"" as ProfNombre,
                       u.""ID_USUARIOS"" as IdUsuario
                from public.""HISTORIAS_CLINICAS"" h
                join public.""PROFESIONALES"" p on p.""ID_PROFESIONALES"" = h.""ID_PROFESIONALES""
                join public.""PERSONAS"" per on per.""ID_PERSONAS"" = p.""ID_PERSONAS""
                left join public.""USUARIOS"" u on u.""ID_PERSONAS"" = per.""ID_PERSONAS""
                where h.""ID_HISTORIAS_CLINICAS"" = @id limit 1";

            dynamic? row = await con.QuerySingleOrDefaultAsync(sql, new { id = int.Parse(id) });
            return row == null ? null : MapRowToDto(row);
        }

        public async Task<MedicalHistoryDto> CreateAsync(CreateMedicalHistoryRequest request, int? createdByUserId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            // Resolver profesional desde usuario
            int? idProfesional = null;
            if (createdByUserId.HasValue)
            {
                const string sqlProf = @"select p.""ID_PROFESIONALES"" from public.""PROFESIONALES"" p
                                          join public.""USUARIOS"" u on u.""ID_PERSONAS"" = p.""ID_PERSONAS""
                                         where u.""ID_USUARIOS"" = @uid limit 1";
                idProfesional = await con.ExecuteScalarAsync<int?>(sqlProf, new { uid = createdByUserId.Value });
            }

            Dictionary<string, object?> meta = new Dictionary<string, object?>
            {
                ["type"] = request.Type,
                ["attachments"] = request.Attachments ?? new List<string>()
            };
            string jsonAdjuntos = JsonConvert.SerializeObject(meta);

            const string sql = @"
                insert into public.""HISTORIAS_CLINICAS"" (
                    ""FECHA"", ""DIAGNOSTICO"", ""SINTOMA"", ""TRATAMIENTO"", ""NOTAS"", ""ADJUNTOS"",
                    ""ID_PACIENTES"", ""ID_PROFESIONALES"", ""ID_TURNOS""
                ) values (
                    @fecha, @diag, @sint, @trat, @notas, @adj,
                    @pid, @idprof, @idturno
                ) returning ""ID_HISTORIAS_CLINICAS""";

            int newId = await con.ExecuteScalarAsync<int>(sql, new
            {
                fecha = request.Date ?? DateTime.UtcNow,
                diag = request.Diagnosis,
                sint = request.Description,
                trat = request.Treatment,
                notas = request.Observations ?? request.Title,
                adj = jsonAdjuntos,
                pid = int.Parse(request.PatientId),
                idprof = idProfesional,
                idturno = string.IsNullOrWhiteSpace(request.AppointmentId) ? (int?)null : (int?)int.Parse(request.AppointmentId)
            });

            MedicalHistoryDto? created = await GetByIdAsync(newId.ToString(), ct) ?? throw new InvalidOperationException("Medical record not found after create");
            // Sobrescribir título (no se guarda explícitamente)
            created.Title = request.Title;
            created.Description = request.Description;
            return created;
        }

        public async Task<MedicalHistoryDto> UpdateAsync(string id, UpdateMedicalHistoryRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            // Obtener json actual de ADJUNTOS
            string? json = await con.ExecuteScalarAsync<string>("select \"ADJUNTOS\" from public.\"HISTORIAS_CLINICAS\" where \"ID_HISTORIAS_CLINICAS\" = @id", new { id = int.Parse(id) });
            Dictionary<string, object?> meta = string.IsNullOrWhiteSpace(json)
                ? new Dictionary<string, object?> { ["type"] = "consultation", ["attachments"] = new List<string>() }
                : JsonConvert.DeserializeObject<Dictionary<string, object?>>(json!) ?? new Dictionary<string, object?>();

            if (request.Attachments != null)
            {
                meta["attachments"] = request.Attachments;
            }

            const string sql = @"
                update public.""HISTORIAS_CLINICAS""
                   set ""DIAGNOSTICO"" = coalesce(@diag, ""DIAGNOSTICO""),
                       ""SINTOMA"" = coalesce(@sint, ""SINTOMA""),
                       ""TRATAMIENTO"" = coalesce(@trat, ""TRATAMIENTO""),
                       ""NOTAS"" = coalesce(@notas, ""NOTAS""),
                       ""ADJUNTOS"" = @adj,
                       ""ULTIMA_ACTUALIZACION"" = NOW()
                 where ""ID_HISTORIAS_CLINICAS"" = @id";

            await con.ExecuteAsync(sql, new
            {
                id = int.Parse(id),
                diag = request.Diagnosis,
                sint = request.Description,
                trat = request.Treatment,
                notas = request.Observations ?? request.Title ?? request.Description,
                adj = JsonConvert.SerializeObject(meta)
            });

            MedicalHistoryDto? updated = await GetByIdAsync(id, ct) ?? throw new InvalidOperationException("Medical record not found after update");
            if (request.Title != null)
            {
                updated.Title = request.Title;
            }
            if (request.Description != null)
            {
                updated.Description = request.Description;
            }
            return updated;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            int affected = await con.ExecuteAsync("delete from public.\"HISTORIAS_CLINICAS\" where \"ID_HISTORIAS_CLINICAS\" = @id", new { id = int.Parse(id) });
            return affected > 0;
        }

        private static MedicalHistoryDto MapRowToDto(dynamic r)
        {
            string? adj = r.Adjuntos as string;
            string type = "consultation";
            List<string> attachments = new List<string>();
            if (!string.IsNullOrWhiteSpace(adj))
            {
                try
                {
                    Dictionary<string, object?>? meta = JsonConvert.DeserializeObject<Dictionary<string, object?>>(adj);
                    if (meta != null)
                    {
                        object? t;
                        if (meta.TryGetValue("type", out t) && t is string ts)
                        {
                            type = ts;
                        }
                        object? att;
                        if (meta.TryGetValue("attachments", out att) && att is Newtonsoft.Json.Linq.JArray jar)
                        {
                            attachments = jar.ToObject<List<string>>() ?? new List<string>();
                        }
                    }
                }
                catch
                {
                }
            }

            return new MedicalHistoryDto
            {
                Id = Convert.ToString(r.Id),
                PatientId = Convert.ToString(r.IdPaciente),
                CreatedBy = Convert.ToString(r.IdUsuario ?? r.IdProf),
                CreatedByName = (string)r.ProfNombre,
                AppointmentId = r.IdTurno is null ? null : Convert.ToString(r.IdTurno),
                Type = type,
                Date = (DateTime)r.Fecha,
                Title = r.Notas as string ?? string.Empty,
                Description = r.Sintoma as string ?? string.Empty,
                Diagnosis = r.Diagnostico as string,
                Treatment = r.Tratamiento as string,
                Observations = r.Notas as string,
                Attachments = attachments,
                CreatedAt = (DateTime)r.Creado,
                UpdatedAt = (DateTime)r.Actualizado
            };
        }
    }
}
