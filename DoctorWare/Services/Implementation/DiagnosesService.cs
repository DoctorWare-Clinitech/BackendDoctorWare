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
    public class DiagnosesService : DoctorWare.Services.Interfaces.IDiagnosesService
    {
        private readonly IDbConnectionFactory factory;

        public DiagnosesService(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task<IEnumerable<DiagnosisDto>> GetByPatientAsync(string patientId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            const string sql = @"
                select h.""ID_HISTORIAS_CLINICAS"" as Id,
                       h.""FECHA"" as Fecha,
                       h.""DIAGNOSTICO"" as Diagnostico,
                       h.""SINTOMA"" as Descripcion,
                       h.""NOTAS"" as Notas,
                       h.""ADJUNTOS"" as Meta,
                       h.""ID_PACIENTES"" as IdPaciente,
                       p.""ID_PROFESIONALES"" as IdProf,
                       u.""ID_USUARIOS"" as IdUsuario,
                       h.""FECHA_CREACION"" as Creado,
                       h.""ULTIMA_ACTUALIZACION"" as Actualizado
                  from public.""HISTORIAS_CLINICAS"" h
                  join public.""PROFESIONALES"" p on p.""ID_PROFESIONALES"" = h.""ID_PROFESIONALES""
                  join public.""PERSONAS"" per on per.""ID_PERSONAS"" = p.""ID_PERSONAS""
                  left join public.""USUARIOS"" u on u.""ID_PERSONAS"" = per.""ID_PERSONAS""
                 where h.""ID_PACIENTES"" = @pid";

            int pid = int.Parse(patientId);
            IEnumerable<dynamic> rows = await con.QueryAsync(sql, new { pid });
            List<DiagnosisDto> list = new List<DiagnosisDto>();
            foreach (var r in rows)
            {
                DiagnosisDto d = MapRow(r);
                if (d.Status != null && ReadType(d) == "diagnosis")
                {
                    list.Add(d);
                }
            }
            return list;
        }

        public async Task<DiagnosisDto> CreateAsync(CreateDiagnosisRequest request, int? createdByUserId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            int? idProfesional = await ResolveProfesionalIdFromUserAsync(con, createdByUserId);
            if (!idProfesional.HasValue)
            {
                throw new ArgumentException("Invalid professional (createdBy)");
            }

            object meta = new
            {
                type = "diagnosis",
                request.Code,
                request.Name,
                request.Description,
                request.Severity,
                request.DiagnosisDate,
                ResolvedDate = (DateTime?)null,
                request.Status,
                request.Notes
            };

            const string sql = @"
                insert into public.""HISTORIAS_CLINICAS"" (
                    ""FECHA"", ""DIAGNOSTICO"", ""SINTOMA"", ""TRATAMIENTO"", ""NOTAS"", ""ADJUNTOS"",
                    ""ID_PACIENTES"", ""ID_PROFESIONALES"", ""ID_TURNOS""
                ) values (
                    @fecha, @diag, @sint, '', @notas, @meta,
                    @pid, @idProf, @idTurno
                ) returning ""ID_HISTORIAS_CLINICAS""";

            int newId = await con.ExecuteScalarAsync<int>(sql, new
            {
                fecha = request.DiagnosisDate,
                diag = request.Name,
                sint = request.Description,
                notas = request.Notes,
                meta = JsonConvert.SerializeObject(meta),
                pid = int.Parse(request.PatientId),
                idProf = idProfesional,
                idTurno = string.IsNullOrWhiteSpace(request.AppointmentId) ? (int?)null : (int?)int.Parse(request.AppointmentId)
            });

            IEnumerable<DiagnosisDto> list = await GetByPatientAsync(request.PatientId, ct);
            DiagnosisDto created = list.First(x => x.Id == newId.ToString());
            return created;
        }

        public async Task<DiagnosisDto> UpdateAsync(string id, UpdateDiagnosisRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            string? json = await con.ExecuteScalarAsync<string>("select \"ADJUNTOS\" from public.\"HISTORIAS_CLINICAS\" where \"ID_HISTORIAS_CLINICAS\" = @id", new { id = int.Parse(id) });
            dynamic meta = string.IsNullOrWhiteSpace(json) ? new { type = "diagnosis" } : JsonConvert.DeserializeObject(json!);
            if (meta.type != "diagnosis")
            {
                throw new InvalidOperationException("Registro no es un diagnóstico");
            }

            string newJson = JsonConvert.SerializeObject(new
            {
                type = "diagnosis",
                Code = request.Code ?? (string?)meta.Code,
                Name = request.Name ?? (string?)meta.Name,
                Description = request.Description ?? (string?)meta.Description,
                Severity = request.Severity ?? (string?)meta.Severity,
                DiagnosisDate = request.DiagnosisDate ?? (DateTime?)meta.DiagnosisDate,
                ResolvedDate = request.ResolvedDate ?? (DateTime?)meta.ResolvedDate,
                Status = request.Status ?? (string?)meta.Status,
                Notes = request.Notes ?? (string?)meta.Notes
            });

            const string sql = @"
                update public.""HISTORIAS_CLINICAS""
                   set ""DIAGNOSTICO"" = coalesce(@diag, ""DIAGNOSTICO""),
                       ""SINTOMA"" = coalesce(@sint, ""SINTOMA""),
                       ""NOTAS"" = coalesce(@notas, ""NOTAS""),
                       ""ADJUNTOS"" = @meta,
                       ""ULTIMA_ACTUALIZACION"" = NOW()
                 where ""ID_HISTORIAS_CLINICAS"" = @id";

            await con.ExecuteAsync(sql, new
            {
                id = int.Parse(id),
                diag = request.Name,
                sint = request.Description,
                notas = request.Notes,
                meta = newJson
            });

            DiagnosisDto? updated = await GetByIdAsync(con, id);
            if (updated == null)
            {
                throw new InvalidOperationException("Diagnóstico no encontrado luego de actualizar");
            }
            return updated;
        }

        private static string ReadType(DiagnosisDto dto)
        {
            return dto.Status != null ? "diagnosis" : string.Empty;
        }

        private async Task<DiagnosisDto?> GetByIdAsync(IDbConnection con, string id)
        {
            const string sql = @"
                select h.""ID_HISTORIAS_CLINICAS"" as Id,
                       h.""FECHA"" as Fecha,
                       h.""DIAGNOSTICO"" as Diagnostico,
                       h.""SINTOMA"" as Descripcion,
                       h.""NOTAS"" as Notas,
                       h.""ADJUNTOS"" as Meta,
                       h.""ID_PACIENTES"" as IdPaciente,
                       p.""ID_PROFESIONALES"" as IdProf,
                       u.""ID_USUARIOS"" as IdUsuario,
                       h.""FECHA_CREACION"" as Creado,
                       h.""ULTIMA_ACTUALIZACION"" as Actualizado
                  from public.""HISTORIAS_CLINICAS"" h
                  join public.""PROFESIONALES"" p on p.""ID_PROFESIONALES"" = h.""ID_PROFESIONALES""
                  join public.""PERSONAS"" per on per.""ID_PERSONAS"" = p.""ID_PERSONAS""
                  left join public.""USUARIOS"" u on u.""ID_PERSONAS"" = per.""ID_PERSONAS""
                 where h.""ID_HISTORIAS_CLINICAS"" = @id limit 1";

            dynamic? row = await con.QuerySingleOrDefaultAsync(sql, new { id = int.Parse(id) });
            return row == null ? null : MapRow(row);
        }

        private static DiagnosisDto MapRow(dynamic r)
        {
            string? metaJson = r.Meta as string;
            string? code = null;
            string name = (string)r.Diagnostico;
            string? description = r.Descripcion as string;
            string severity = "low";
            DateTime diagnosisDate = (DateTime)r.Fecha;
            DateTime? resolvedDate = null;
            string status = "active";
            string? notes = r.Notas as string;

            if (!string.IsNullOrWhiteSpace(metaJson))
            {
                try
                {
                    dynamic meta = JsonConvert.DeserializeObject(metaJson!);
                    code = meta.Code;
                    name = meta.Name ?? name;
                    description = meta.Description ?? description;
                    severity = meta.Severity ?? severity;
                    diagnosisDate = meta.DiagnosisDate ?? diagnosisDate;
                    resolvedDate = meta.ResolvedDate ?? resolvedDate;
                    status = meta.Status ?? status;
                    notes = meta.Notes ?? notes;
                }
                catch
                {
                }
            }

            DiagnosisDto dto = new DiagnosisDto
            {
                Id = Convert.ToString(r.Id),
                PatientId = Convert.ToString(r.IdPaciente),
                ProfessionalId = Convert.ToString(r.IdUsuario ?? r.IdProf),
                AppointmentId = null,
                Code = code,
                Name = name,
                Description = description,
                Severity = severity,
                DiagnosisDate = diagnosisDate,
                ResolvedDate = resolvedDate,
                Status = status,
                Notes = notes,
                CreatedAt = (DateTime)r.Creado,
                UpdatedAt = (DateTime)r.Actualizado
            };
            return dto;
        }

        private static async Task<int?> ResolveProfesionalIdFromUserAsync(IDbConnection con, int? userId)
        {
            if (!userId.HasValue)
            {
                return null;
            }
            const string sqlProf = @"select p.""ID_PROFESIONALES"" from public.""PROFESIONALES"" p
                                      join public.""USUARIOS"" u on u.""ID_PERSONAS"" = p.""ID_PERSONAS""
                                     where u.""ID_USUARIOS"" = @uid limit 1";
            int? idProf = await con.ExecuteScalarAsync<int?>(sqlProf, new { uid = userId.Value });
            return idProf;
        }
    }
}
