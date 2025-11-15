using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Requests.Appointments;
using DoctorWare.DTOs.Response.Appointments;
using DoctorWare.Services.Implementation.Helpers;
using DoctorWare.Services.Interfaces;
using System.Data;
using System.Linq;
using System;

namespace DoctorWare.Services.Implementation
{
    public class AppointmentsService : IAppointmentsService
    {
        private readonly IDbConnectionFactory factory;

        public AppointmentsService(IDbConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAsync(
            string? professionalUserId,
            string? patientId,
            DateTime? startDate,
            DateTime? endDate,
            string? status,
            string? type,
            CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            int? idProfesionales = await ProfessionalResolver.TryResolveAsync(con, professionalUserId, ct);

            List<string> filters = new List<string>();
            DynamicParameters parameters = new DynamicParameters();

            if (idProfesionales.HasValue)
            {
                filters.Add("t.\"ID_PROFESIONALES\" = @idProf");
                parameters.Add("idProf", idProfesionales.Value);
            }
            if (!string.IsNullOrWhiteSpace(patientId))
            {
                int parsedPid;
                if (int.TryParse(patientId, out parsedPid))
                {
                    filters.Add("t.\"ID_PACIENTES\" = @pid");
                    parameters.Add("pid", parsedPid);
                }
            }
            if (startDate.HasValue)
            {
                filters.Add("t.\"FECHA\" >= @start");
                parameters.Add("start", startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                filters.Add("t.\"FECHA\" <= @end");
                parameters.Add("end", endDate.Value.Date);
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                // Mapear a nombre DB y filtrar por NOMBRE en ESTADOS_TURNO
                string dbStatus = AppointmentMappingHelper.MapFrontStatusToDb(status);
                filters.Add("et.\"NOMBRE\" = @statusNombre");
                parameters.Add("statusNombre", dbStatus);
            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                string dbType = AppointmentMappingHelper.MapFrontTypeToDb(type);
                filters.Add("tt.\"NOMBRE\" = @tipoNombre");
                parameters.Add("tipoNombre", dbType);
            }

            string where = filters.Count > 0 ? (" where " + string.Join(" and ", filters)) : string.Empty;

            string sql = $@"
                select
                    t.""ID_TURNOS"" as IdTurno,
                    t.""FECHA"" as Fecha,
                    t.""HORA_INICIO"" as HoraInicio,
                    t.""HORA_FIN"" as HoraFin,
                    coalesce(t.""DURACION"", p.""DURACION_TURNO_EN_MINUTOS"") as Duracion,
                    t.""MOTIVO_CONSULTA"" as Motivo,
                    t.""NOTA_ADICIONAL"" as Nota,
                    t.""OBSERVACION_PROFECIONAL"" as ObsProfesional,
                    t.""FECHA_CANCELACION"" as FechaCancelacion,
                    t.""MOTIVO_CANCELACION"" as MotivoCancelacion,
                    t.""FECHA_CREACION"" as FechaCreacion,
                    t.""ULTIMA_ACTUALIZACION"" as UltimaActualizacion,
                    pa.""ID_PACIENTES"" as IdPaciente,
                    perPac.""NOMBRE"" || ' ' || perPac.""APELLIDO"" as PacienteNombre,
                    prof.""ID_PROFESIONALES"" as IdProfesional,
                    u.""ID_USUARIOS"" as IdUsuarioProfesional,
                    perPro.""NOMBRE"" || ' ' || perPro.""APELLIDO"" as ProfesionalNombre,
                    et.""NOMBRE"" as EstadoNombre,
                    tt.""NOMBRE"" as TipoNombre,
                    t.""ID_USUARIO_CREACION"" as IdUsuarioCreacion
                from public.""TURNOS"" t
                join public.""PACIENTES"" pa on pa.""ID_PACIENTES"" = t.""ID_PACIENTES""
                join public.""PERSONAS"" perPac on perPac.""ID_PERSONAS"" = pa.""ID_PERSONAS""
                join public.""PROFESIONALES"" prof on prof.""ID_PROFESIONALES"" = t.""ID_PROFESIONALES""
                join public.""PERSONAS"" perPro on perPro.""ID_PERSONAS"" = prof.""ID_PERSONAS""
                left join public.""USUARIOS"" u on u.""ID_PERSONAS"" = prof.""ID_PERSONAS""
                join public.""ESTADOS_TURNO"" et on et.""ID_ESTADOS_TURNO"" = t.""ID_ESTADOS_TURNO""
                join public.""TIPOS_TURNO"" tt on tt.""ID_TIPOS_TURNO"" = t.""ID_TIPOS_TURNO""{where}
                order by t.""FECHA"" desc, t.""HORA_INICIO"" desc";

            IEnumerable<dynamic> rows = await con.QueryAsync(sql, parameters);

            List<AppointmentDto> list = rows.Select(r => new AppointmentDto
            {
                Id = Convert.ToString(r.IdTurno),
                PatientId = Convert.ToString(r.IdPaciente),
                PatientName = (string)r.PacienteNombre,
                ProfessionalId = Convert.ToString(r.IdUsuarioProfesional ?? r.IdProfesional),
                ProfessionalName = (string)r.ProfesionalNombre,
                Date = (DateTime)r.Fecha,
                StartTime = AppointmentMappingHelper.FormatTimeSpan((TimeSpan)r.HoraInicio),
                EndTime = AppointmentMappingHelper.FormatTimeSpan((TimeSpan)r.HoraFin),
                Duration = Convert.ToInt32(r.Duracion),
                Status = AppointmentMappingHelper.MapDbStatusToFront((string)r.EstadoNombre),
                Type = AppointmentMappingHelper.MapDbTypeToFront((string)r.TipoNombre),
                Reason = r.Motivo as string,
                Notes = r.Nota as string,
                Observations = r.ObsProfesional as string,
                CreatedBy = Convert.ToString(r.IdUsuarioCreacion ?? ""),
                CreatedAt = (DateTime)r.FechaCreacion,
                UpdatedAt = (DateTime)r.UltimaActualizacion,
                CancelledAt = r.FechaCancelacion as DateTime?,
                CancelledBy = null,
                CancellationReason = r.MotivoCancelacion as string
            }).ToList();

            return list;
        }

        public async Task<AppointmentDto?> GetByIdAsync(string id, CancellationToken ct)
        {
            IEnumerable<AppointmentDto> items = await GetAsync(null, null, null, null, null, null, ct);
            return items.FirstOrDefault(a => a.Id == id);
        }

        public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, int? createdByUserId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            int idPaciente;
            if (!int.TryParse(request.PatientId, out idPaciente))
            {
                throw new ArgumentException("Invalid patientId");
            }
            int idProfesional = await ProfessionalResolver.ResolveRequiredAsync(con, request.ProfessionalId, ct);

            string tipoNombre = AppointmentMappingHelper.MapFrontTypeToDb(request.Type);
            string estadoNombre = AppointmentMappingHelper.MapFrontStatusToDb("scheduled");

            int idTipo = await con.ExecuteScalarAsync<int>("select \"ID_TIPOS_TURNO\" from public.\"TIPOS_TURNO\" where \"NOMBRE\" = @n limit 1", new { n = tipoNombre });
            int idEstado = await con.ExecuteScalarAsync<int>("select \"ID_ESTADOS_TURNO\" from public.\"ESTADOS_TURNO\" where \"NOMBRE\" = @n limit 1", new { n = estadoNombre });

            TimeSpan start = TimeSpan.Parse(request.StartTime);
            TimeSpan end = start.Add(TimeSpan.FromMinutes(request.Duration));

            const string insertSql = @"
                insert into public.""TURNOS"" (
                    ""FECHA"", ""HORA_INICIO"", ""HORA_FIN"", ""DURACION"",
                    ""MOTIVO_CONSULTA"", ""NOTA_ADICIONAL"",
                    ""ID_PACIENTES"", ""ID_PROFESIONALES"", ""ID_ESTADOS_TURNO"", ""ID_TIPOS_TURNO"", ""ID_USUARIO_CREACION""
                ) values (
                    @fecha, @horaInicio, @horaFin, @duracion, @motivo, @nota,
                    @idPac, @idProf, @idEstado, @idTipo, @idUser
                ) returning ""ID_TURNOS""";

            int newId = await con.ExecuteScalarAsync<int>(insertSql, new
            {
                fecha = request.Date.Date,
                horaInicio = start,
                horaFin = end,
                duracion = request.Duration,
                motivo = request.Reason,
                nota = request.Notes,
                idPac = idPaciente,
                idProf = idProfesional,
                idEstado = idEstado,
                idTipo = idTipo,
                idUser = createdByUserId
            });

            AppointmentDto? created = await GetByIdAsync(newId.ToString(), ct) ?? throw new InvalidOperationException("Created appointment not found");
            return created;
        }

        public async Task<AppointmentDto> UpdateAsync(string id, UpdateAppointmentRequest request, int? updatedByUserId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            List<string> setParts = new List<string>();
            DynamicParameters p = new DynamicParameters();
            p.Add("id", int.Parse(id));

            if (request.Date.HasValue)
            {
                setParts.Add("\"FECHA\" = @fecha");
                p.Add("fecha", request.Date.Value.Date);
            }
            if (!string.IsNullOrWhiteSpace(request.StartTime))
            {
                setParts.Add("\"HORA_INICIO\" = @horaInicio");
                p.Add("horaInicio", TimeSpan.Parse(request.StartTime));
                if (request.Duration.HasValue)
                {
                    setParts.Add("\"HORA_FIN\" = @horaFin");
                    p.Add("horaFin", TimeSpan.Parse(request.StartTime).Add(TimeSpan.FromMinutes(request.Duration.Value)));
                }
            }
            if (request.Duration.HasValue)
            {
                setParts.Add("\"DURACION\" = @duracion");
                p.Add("duracion", request.Duration.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                string dbStatus = AppointmentMappingHelper.MapFrontStatusToDb(request.Status);
                int idEstado = await con.ExecuteScalarAsync<int>("select \"ID_ESTADOS_TURNO\" from public.\"ESTADOS_TURNO\" where \"NOMBRE\" = @n limit 1", new { n = dbStatus });
                setParts.Add("\"ID_ESTADOS_TURNO\" = @idEstado");
                p.Add("idEstado", idEstado);
            }
            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                string dbTipo = AppointmentMappingHelper.MapFrontTypeToDb(request.Type);
                int idTipo = await con.ExecuteScalarAsync<int>("select \"ID_TIPOS_TURNO\" from public.\"TIPOS_TURNO\" where \"NOMBRE\" = @n limit 1", new { n = dbTipo });
                setParts.Add("\"ID_TIPOS_TURNO\" = @idTipo");
                p.Add("idTipo", idTipo);
            }
            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                setParts.Add("\"MOTIVO_CONSULTA\" = @motivo");
                p.Add("motivo", request.Reason);
            }
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                setParts.Add("\"NOTA_ADICIONAL\" = @nota");
                p.Add("nota", request.Notes);
            }
            if (!string.IsNullOrWhiteSpace(request.Observations))
            {
                setParts.Add("\"OBSERVACION_PROFECIONAL\" = @obs");
                p.Add("obs", request.Observations);
            }

            setParts.Add("\"ULTIMA_ACTUALIZACION\" = NOW()");

            string setClause = string.Join(", ", setParts);
            string sql = $"update public.\"TURNOS\" set {setClause} where \"ID_TURNOS\" = @id";
            await con.ExecuteAsync(sql, p);

            AppointmentDto? updated = await GetByIdAsync(id, ct) ?? throw new InvalidOperationException("Appointment not found after update");
            return updated;
        }

        public async Task<bool> CancelAsync(string id, int? cancelledByUserId, string? reason, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            string estadoNombre = AppointmentMappingHelper.MapFrontStatusToDb("cancelled");
            int idEstado = await con.ExecuteScalarAsync<int>("select \"ID_ESTADOS_TURNO\" from public.\"ESTADOS_TURNO\" where \"NOMBRE\" = @n limit 1", new { n = estadoNombre });

            const string sql = @"
                update public.""TURNOS""
                   set ""ID_ESTADOS_TURNO"" = @idEstado,
                       ""FECHA_CANCELACION"" = NOW(),
                       ""MOTIVO_CANCELACION"" = coalesce(@motivo, 'Cancelado via API'),
                       ""ULTIMA_ACTUALIZACION"" = NOW()
                 where ""ID_TURNOS"" = @id";

            int affected = await con.ExecuteAsync(sql, new { id = int.Parse(id), idEstado, motivo = reason });
            return affected > 0;
        }

        public async Task<(int total, int scheduled, int confirmed, int completed, int cancelled, int noShow)> GetStatsAsync(string? professionalUserId, CancellationToken ct)
        {
            IEnumerable<AppointmentDto> items = await GetAsync(professionalUserId, null, null, null, null, null, ct);
            int total = items.Count();
            int scheduled = items.Count(a => a.Status == "scheduled");
            int confirmed = items.Count(a => a.Status == "confirmed");
            int completed = items.Count(a => a.Status == "completed");
            int cancelled = items.Count(a => a.Status == "cancelled");
            int noShow = items.Count(a => a.Status == "no_show");
            return (total, scheduled, confirmed, completed, cancelled, noShow);
        }

    }
}
