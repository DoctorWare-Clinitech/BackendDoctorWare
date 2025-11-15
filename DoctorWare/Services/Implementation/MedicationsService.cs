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
    public class MedicationsService : DoctorWare.Services.Interfaces.IMedicationsService
    {
        private readonly IDbConnectionFactory factory;

        public MedicationsService(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task<IEnumerable<MedicationDto>> GetByPatientAsync(string patientId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            const string sql = @"
                select h.""ID_HISTORIAS_CLINICAS"" as Id,
                       h.""FECHA"" as Fecha,
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
            List<MedicationDto> list = new List<MedicationDto>();
            foreach (var r in rows)
            {
                MedicationDto m = MapRow(r);
                if (ReadType(m) == "medication")
                {
                    list.Add(m);
                }
            }
            return list;
        }

        public async Task<MedicationDto> CreateAsync(CreateMedicationRequest request, int? createdByUserId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            int? idProfesional = await ResolveProfesionalIdFromUserAsync(con, createdByUserId);
            if (!idProfesional.HasValue)
            {
                throw new ArgumentException("Invalid professional (createdBy)");
            }

            object meta = new
            {
                type = "medication",
                request.MedicationName,
                request.Dosage,
                request.Frequency,
                request.Duration,
                request.StartDate,
                request.EndDate,
                request.Instructions,
                Active = true
            };

            const string sql = @"
                insert into public.""HISTORIAS_CLINICAS"" (
                    ""FECHA"", ""DIAGNOSTICO"", ""SINTOMA"", ""TRATAMIENTO"", ""NOTAS"", ""ADJUNTOS"",
                    ""ID_PACIENTES"", ""ID_PROFESIONALES"", ""ID_TURNOS""
                ) values (
                    @fecha, '', '', '', @notas, @meta,
                    @pid, @idProf, @idTurno
                ) returning ""ID_HISTORIAS_CLINICAS""";

            int newId = await con.ExecuteScalarAsync<int>(sql, new
            {
                fecha = request.StartDate,
                notas = request.Instructions,
                meta = JsonConvert.SerializeObject(meta),
                pid = int.Parse(request.PatientId),
                idProf = idProfesional,
                idTurno = string.IsNullOrWhiteSpace(request.AppointmentId) ? (int?)null : (int?)int.Parse(request.AppointmentId)
            });

            IEnumerable<MedicationDto> list = await GetByPatientAsync(request.PatientId, ct);
            MedicationDto created = list.First(x => x.Id == newId.ToString());
            return created;
        }

        public async Task<MedicationDto> UpdateAsync(string id, UpdateMedicationRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            string? json = await con.ExecuteScalarAsync<string>("select \"ADJUNTOS\" from public.\"HISTORIAS_CLINICAS\" where \"ID_HISTORIAS_CLINICAS\" = @id", new { id = int.Parse(id) });
            dynamic meta = string.IsNullOrWhiteSpace(json) ? new { type = "medication", Active = true } : JsonConvert.DeserializeObject(json!);
            if (meta.type != "medication")
            {
                throw new InvalidOperationException("Registro no es un medicamento");
            }

            string newJson = JsonConvert.SerializeObject(new
            {
                type = "medication",
                MedicationName = request.MedicationName ?? (string?)meta.MedicationName,
                Dosage = request.Dosage ?? (string?)meta.Dosage,
                Frequency = request.Frequency ?? (string?)meta.Frequency,
                Duration = request.Duration ?? (string?)meta.Duration,
                StartDate = request.StartDate ?? (DateTime?)meta.StartDate,
                EndDate = request.EndDate ?? (DateTime?)meta.EndDate,
                Instructions = request.Instructions ?? (string?)meta.Instructions,
                Active = request.Active ?? (bool)meta.Active
            });

            const string sql = @"
                update public.""HISTORIAS_CLINICAS""
                   set ""NOTAS"" = coalesce(@notas, ""NOTAS""),
                       ""ADJUNTOS"" = @meta,
                       ""ULTIMA_ACTUALIZACION"" = NOW()
                 where ""ID_HISTORIAS_CLINICAS"" = @id";
            await con.ExecuteAsync(sql, new
            {
                id = int.Parse(id),
                notas = request.Instructions,
                meta = newJson
            });

            MedicationDto? updated = await GetByIdAsync(con, id);
            if (updated == null)
            {
                throw new InvalidOperationException("Medicamento no encontrado luego de actualizar");
            }
            return updated;
        }

        public async Task<MedicationDto> DiscontinueAsync(string id, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            string? json = await con.ExecuteScalarAsync<string>("select \"ADJUNTOS\" from public.\"HISTORIAS_CLINICAS\" where \"ID_HISTORIAS_CLINICAS\" = @id", new { id = int.Parse(id) });
            dynamic meta = string.IsNullOrWhiteSpace(json) ? new { type = "medication", Active = true } : JsonConvert.DeserializeObject(json!);
            if (meta.type != "medication")
            {
                throw new InvalidOperationException("Registro no es un medicamento");
            }
            string newJson = JsonConvert.SerializeObject(new
            {
                type = "medication",
                MedicationName = (string?)meta.MedicationName,
                Dosage = (string?)meta.Dosage,
                Frequency = (string?)meta.Frequency,
                Duration = (string?)meta.Duration,
                StartDate = (DateTime?)meta.StartDate,
                EndDate = DateTime.UtcNow,
                Instructions = (string?)meta.Instructions,
                Active = false
            });
            await con.ExecuteAsync("update public.\"HISTORIAS_CLINICAS\" set \"ADJUNTOS\" = @meta, \"ULTIMA_ACTUALIZACION\" = NOW() where \"ID_HISTORIAS_CLINICAS\" = @id", new { id = int.Parse(id), meta = newJson });
            MedicationDto? updated = await GetByIdAsync(con, id);
            if (updated == null)
            {
                throw new InvalidOperationException("Medicamento no encontrado");
            }
            return updated;
        }

        private static MedicationDto MapRow(dynamic r)
        {
            string? metaJson = r.Meta as string;
            string medicationName = string.Empty;
            string dosage = string.Empty;
            string frequency = string.Empty;
            string duration = string.Empty;
            DateTime startDate = (DateTime)r.Fecha;
            DateTime? endDate = null;
            string? instructions = r.Notas as string;
            bool active = true;
            if (!string.IsNullOrWhiteSpace(metaJson))
            {
                try
                {
                    dynamic meta = JsonConvert.DeserializeObject(metaJson!);
                    if (meta.type == "medication")
                    {
                        medicationName = meta.MedicationName ?? medicationName;
                        dosage = meta.Dosage ?? dosage;
                        frequency = meta.Frequency ?? frequency;
                        duration = meta.Duration ?? duration;
                        startDate = meta.StartDate ?? startDate;
                        endDate = meta.EndDate ?? endDate;
                        instructions = meta.Instructions ?? instructions;
                        active = meta.Active ?? active;
                    }
                }
                catch
                {
                }
            }

            MedicationDto dto = new MedicationDto
            {
                Id = Convert.ToString(r.Id),
                PatientId = Convert.ToString(r.IdPaciente),
                ProfessionalId = Convert.ToString(r.IdUsuario ?? r.IdProf),
                AppointmentId = null,
                MedicationName = medicationName,
                Dosage = dosage,
                Frequency = frequency,
                Duration = duration,
                StartDate = startDate,
                EndDate = endDate,
                Instructions = instructions,
                Active = active,
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

        private static string ReadType(MedicationDto dto)
        {
            return dto.Active ? "medication" : "medication";
        }
        
        private static async Task<MedicationDto?> GetByIdAsync(IDbConnection con, string id)
        {
            const string sql = @"
                select h.""ID_HISTORIAS_CLINICAS"" as Id,
                       h.""FECHA"" as Fecha,
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
    }
}
