using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Requests.Schedule;
using DoctorWare.DTOs.Response.Frontend;
using DoctorWare.Exceptions;
using DoctorWare.Services.Implementation.Helpers;
using Microsoft.Extensions.Logging;

namespace DoctorWare.Services.Implementation
{
    public class ScheduleService : DoctorWare.Services.Interfaces.IScheduleService
    {
        private readonly IDbConnectionFactory factory;
        private readonly ILogger<ScheduleService> logger;

        public ScheduleService(IDbConnectionFactory factory, ILogger<ScheduleService> logger)
        {
            this.factory = factory;
            this.logger = logger;
        }

        public async Task<ScheduleConfigDto> GetConfigAsync(string professionalUserId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            int idProf = await ProfessionalResolver.ResolveRequiredAsync(con, professionalUserId, ct);
            logger.LogInformation("Obteniendo configuración de agenda para usuario profesional {UserId} (ID_PROFESIONALES={ProfesionalId})", professionalUserId, idProf);
            int consultationDuration = await GetDefaultDurationAsync(con, idProf, ct);

            const string sqlSlots = @"
                select
                    ""ID_AGENDA_PROFESIONALES"" as Id,
                    ""DIA_SEMANA"" as DiaSemana,
                    ""HORA_INICIO"" as HoraInicio,
                    ""HORA_FIN"" as HoraFin,
                    ""DURACION_MINUTOS"" as DuracionMinutos,
                    ""ES_LABORABLE"" as EsLaborable
                from public.""AGENDA_PROFESIONALES""
                where ""ID_PROFESIONALES"" = @idProf
                order by ""DIA_SEMANA"", ""HORA_INICIO"";";

            IEnumerable<AgendaRow> rows = await con.QueryAsync<AgendaRow>(sqlSlots, new { idProf });

            List<ScheduleTimeSlotDto> slots = rows
                .Select(r => new ScheduleTimeSlotDto
                {
                    Id = r.Id.ToString(CultureInfo.InvariantCulture),
                    DayOfWeek = r.DiaSemana,
                    StartTime = r.HoraInicio.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                    EndTime = r.HoraFin.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                    Duration = r.DuracionMinutos ?? consultationDuration,
                    IsActive = r.EsLaborable
                })
                .ToList();

            ScheduleConfigDto config = new ScheduleConfigDto
            {
                ProfessionalId = professionalUserId,
                TimeSlots = slots,
                BlockedSlots = new List<ScheduleBlockedSlotDto>(),
                ConsultationDuration = consultationDuration
            };

            logger.LogDebug("Configuración de agenda recuperada para profesional {ProfesionalId}: Slots={Slots}, DuraciónPorDefecto={Duracion}",
                idProf,
                slots.Count,
                consultationDuration);

            return config;
        }

        public async Task<ScheduleConfigDto> UpdateConfigAsync(string professionalUserId, UpdateScheduleConfigRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            con.Open();
            using IDbTransaction tx = con.BeginTransaction();

            try
            {
                int idProf = await ProfessionalResolver.ResolveRequiredAsync(con, professionalUserId, ct, tx);

                logger.LogInformation("Actualizando configuración de agenda para usuario profesional {UserId} (ID_PROFESIONALES={ProfesionalId})",
                    professionalUserId,
                    idProf);

                if (request.ConsultationDuration.HasValue && request.ConsultationDuration.Value > 0)
                {
                    const string sqlUpdate = @"
                        update public.""PROFESIONALES""
                           set ""DURACION_TURNO_EN_MINUTOS"" = @dur,
                               ""ULTIMA_ACTUALIZACION"" = NOW()
                         where ""ID_PROFESIONALES"" = @id;";

                    await con.ExecuteAsync(sqlUpdate, new { id = idProf, dur = request.ConsultationDuration.Value }, tx);
                }

                if (request.TimeSlots is not null)
                {
                    const string sqlDelete = @"delete from public.""AGENDA_PROFESIONALES"" where ""ID_PROFESIONALES"" = @idProf;";
                    await con.ExecuteAsync(sqlDelete, new { idProf }, tx);

                    foreach (TimeSlotConfigRequest slot in request.TimeSlots)
                    {
                        TimeSpan start = ParseTime(slot.StartTime);
                        TimeSpan end = ParseTime(slot.EndTime);
                        if (start >= end)
                        {
                            throw new BadRequestException("El horario de inicio debe ser menor al de fin.");
                        }

                        const string sqlInsert = @"
                            insert into public.""AGENDA_PROFESIONALES""
                                (""ID_PROFESIONALES"", ""DIA_SEMANA"", ""HORA_INICIO"", ""HORA_FIN"", ""DURACION_MINUTOS"", ""ES_LABORABLE"")
                            values
                                (@idProf, @dia, @inicio, @fin, @dur, @lab);";

                        await con.ExecuteAsync(sqlInsert, new
                        {
                            idProf,
                            dia = slot.DayOfWeek,
                            inicio = start,
                            fin = end,
                            dur = slot.Duration,
                            lab = slot.IsActive
                        }, tx);
                    }
                }

                tx.Commit();

                logger.LogInformation("Configuración de agenda actualizada para profesional {ProfesionalId}. NuevaDuraciónPorDefecto={Duracion}, SlotsDefinidos={Slots}",
                    idProf,
                    request.ConsultationDuration ?? 0,
                    request.TimeSlots?.Count ?? 0);
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            return await GetConfigAsync(professionalUserId, ct);
        }

        public async Task<ScheduleTimeSlotDto> AddTimeSlotAsync(string professionalUserId, CreateTimeSlotRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            con.Open();

            int idProf = await ProfessionalResolver.ResolveRequiredAsync(con, professionalUserId, ct);
            int defaultDuration = await GetDefaultDurationAsync(con, idProf, ct);

            TimeSpan start = ParseTime(request.StartTime);
            TimeSpan end = ParseTime(request.EndTime);
            if (start >= end)
            {
                throw new BadRequestException("El horario de inicio debe ser menor al de fin.");
            }

            int duration = request.Duration > 0 ? request.Duration : defaultDuration;

            const string sqlInsert = @"
                insert into public.""AGENDA_PROFESIONALES""
                    (""ID_PROFESIONALES"", ""DIA_SEMANA"", ""HORA_INICIO"", ""HORA_FIN"", ""DURACION_MINUTOS"", ""ES_LABORABLE"")
                values
                    (@idProf, @dia, @inicio, @fin, @dur, @lab)
                returning ""ID_AGENDA_PROFESIONALES"";";

            int idSlot = await con.ExecuteScalarAsync<int>(sqlInsert, new
            {
                idProf,
                dia = request.DayOfWeek,
                inicio = start,
                fin = end,
                dur = duration,
                lab = request.IsActive
            });

            ScheduleTimeSlotDto slotDto = await GetTimeSlotAsync(con, idProf, idSlot, ct);
            logger.LogInformation(
                "Horario agregado para profesional {ProfesionalId}: SlotId={SlotId}, DiaSemana={Dia}, Inicio={Inicio}, Fin={Fin}, Duracion={Duracion} minutos, Activo={Activo}",
                idProf,
                idSlot,
                slotDto.DayOfWeek,
                slotDto.StartTime,
                slotDto.EndTime,
                slotDto.Duration,
                slotDto.IsActive);
            return slotDto;
        }

        public async Task<ScheduleTimeSlotDto> UpdateTimeSlotAsync(string professionalUserId, string slotId, UpdateTimeSlotRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            con.Open();

            int idProf = await ProfessionalResolver.ResolveRequiredAsync(con, professionalUserId, ct);
            if (!int.TryParse(slotId, out int idSlot))
            {
                throw new BadRequestException("Identificador de horario inválido.");
            }

            List<string> setParts = new List<string>();
            DynamicParameters p = new DynamicParameters();
            p.Add("idProf", idProf);
            p.Add("idSlot", idSlot);

            if (request.DayOfWeek.HasValue)
            {
                setParts.Add(@"""DIA_SEMANA"" = @dia");
                p.Add("dia", request.DayOfWeek.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.StartTime))
            {
                TimeSpan start = ParseTime(request.StartTime);
                setParts.Add(@"""HORA_INICIO"" = @inicio");
                p.Add("inicio", start);
            }
            if (!string.IsNullOrWhiteSpace(request.EndTime))
            {
                TimeSpan end = ParseTime(request.EndTime);
                setParts.Add(@"""HORA_FIN"" = @fin");
                p.Add("fin", end);
            }
            if (request.Duration.HasValue && request.Duration.Value > 0)
            {
                setParts.Add(@"""DURACION_MINUTOS"" = @dur");
                p.Add("dur", request.Duration.Value);
            }
            if (request.IsActive.HasValue)
            {
                setParts.Add(@"""ES_LABORABLE"" = @lab");
                p.Add("lab", request.IsActive.Value);
            }

            if (setParts.Count > 0)
            {
                setParts.Add(@"""ULTIMA_ACTUALIZACION"" = NOW()");
                string sql = $@"
                    update public.""AGENDA_PROFESIONALES""
                       set {string.Join(", ", setParts)}
                     where ""ID_AGENDA_PROFESIONALES"" = @idSlot
                       and ""ID_PROFESIONALES"" = @idProf;";

                int affected = await con.ExecuteAsync(sql, p);
                if (affected == 0)
                {
                    throw new NotFoundException("Horario no encontrado.");
                }
            }

            ScheduleTimeSlotDto slotDto = await GetTimeSlotAsync(con, idProf, idSlot, ct);
            logger.LogInformation(
                "Horario actualizado para profesional {ProfesionalId}: SlotId={SlotId}, DiaSemana={Dia}, Inicio={Inicio}, Fin={Fin}, Duracion={Duracion} minutos, Activo={Activo}",
                idProf,
                idSlot,
                slotDto.DayOfWeek,
                slotDto.StartTime,
                slotDto.EndTime,
                slotDto.Duration,
                slotDto.IsActive);
            return slotDto;
        }

        public async Task DeleteTimeSlotAsync(string professionalUserId, string slotId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            int idProf = await ProfessionalResolver.ResolveRequiredAsync(con, professionalUserId, ct);
            if (!int.TryParse(slotId, out int idSlot))
            {
                throw new BadRequestException("Identificador de horario inválido.");
            }

            const string sql = @"
                delete from public.""AGENDA_PROFESIONALES""
                 where ""ID_AGENDA_PROFESIONALES"" = @idSlot
                   and ""ID_PROFESIONALES"" = @idProf;";

            int affected = await con.ExecuteAsync(sql, new { idSlot, idProf });
            if (affected == 0)
            {
                throw new NotFoundException("Horario no encontrado.");
            }

            logger.LogInformation(
                "Horario eliminado para profesional {ProfesionalId}: SlotId={SlotId}",
                idProf,
                idSlot);
        }

        public async Task<ScheduleBlockedSlotDto> AddBlockedSlotAsync(string professionalUserId, CreateBlockedSlotRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            con.Open();

            int idProf = await ProfessionalResolver.ResolveRequiredAsync(con, professionalUserId, ct);

            DateTime dateOnly = request.Date.Date;
            TimeSpan start = ParseTime(request.StartTime);
            TimeSpan end = ParseTime(request.EndTime);
            if (start >= end)
            {
                throw new BadRequestException("El horario de inicio debe ser menor al de fin.");
            }

            const string sqlInsert = @"
                insert into public.""AUSENCIAS_PROFESIONALES""
                    (""ID_PROFESIONALES"", ""FECHA_DESDE"", ""FECHA_HASTA"", ""HORA_DESDE"", ""HORA_HASTA"", ""TIPO"", ""MOTIVO"")
                values
                    (@idProf, @desde, @hasta, @hDesde, @hHasta, @tipo, @motivo)
                returning ""ID_AUSENCIAS_PROFESIONALES"", ""FECHA_CREACION"";";

            (int idBlock, DateTime createdAt) = await con.QuerySingleAsync<(int, DateTime)>(sqlInsert, new
            {
                idProf,
                desde = dateOnly,
                hasta = dateOnly,
                hDesde = start,
                hHasta = end,
                tipo = request.Type,
                motivo = request.Reason
            });

            ScheduleBlockedSlotDto dto = new ScheduleBlockedSlotDto
            {
                Id = idBlock.ToString(CultureInfo.InvariantCulture),
                Date = dateOnly,
                StartTime = start.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                EndTime = end.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                Reason = request.Reason,
                CreatedAt = createdAt
            };

            logger.LogInformation(
                "Bloqueo de agenda creado para profesional {ProfesionalId}: BlockId={BlockId}, Fecha={Fecha}, Inicio={Inicio}, Fin={Fin}, Motivo={Motivo}",
                idProf,
                idBlock,
                dto.Date,
                dto.StartTime,
                dto.EndTime,
                dto.Reason);

            return dto;
        }

        public async Task DeleteBlockedSlotAsync(string professionalUserId, string blockId, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            int idProf = await ProfessionalResolver.ResolveRequiredAsync(con, professionalUserId, ct);
            if (!int.TryParse(blockId, out int idBlock))
            {
                throw new BadRequestException("Identificador de bloqueo inválido.");
            }

            const string sql = @"
                delete from public.""AUSENCIAS_PROFESIONALES""
                 where ""ID_AUSENCIAS_PROFESIONALES"" = @idBlock
                   and ""ID_PROFESIONALES"" = @idProf;";

            int affected = await con.ExecuteAsync(sql, new { idBlock, idProf });
            if (affected == 0)
            {
                throw new NotFoundException("Bloqueo no encontrado.");
            }

            logger.LogInformation(
                "Bloqueo de agenda eliminado para profesional {ProfesionalId}: BlockId={BlockId}",
                idProf,
                idBlock);
        }

        public async Task<IEnumerable<ScheduleBlockedSlotDto>> GetBlockedSlotsAsync(string professionalUserId, DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            int idProf = await ProfessionalResolver.ResolveRequiredAsync(con, professionalUserId, ct);
            DateTime from = startDate.Date;
            DateTime to = endDate.Date;

            const string sql = @"
                select
                    ""ID_AUSENCIAS_PROFESIONALES"" as Id,
                    ""FECHA_DESDE"" as FechaDesde,
                    ""FECHA_HASTA"" as FechaHasta,
                    ""HORA_DESDE"" as HoraDesde,
                    ""HORA_HASTA"" as HoraHasta,
                    ""MOTIVO"" as Motivo,
                    ""FECHA_CREACION"" as FechaCreacion
                from public.""AUSENCIAS_PROFESIONALES""
                where ""ID_PROFESIONALES"" = @idProf
                  and ""FECHA_DESDE"" <= @to
                  and ""FECHA_HASTA"" >= @from
                order by ""FECHA_DESDE"", ""HORA_DESDE"";";

            IEnumerable<AbsenceRow> rows = await con.QueryAsync<AbsenceRow>(sql, new { idProf, from, to });

            List<ScheduleBlockedSlotDto> result = rows
                .Select(r =>
                {
                    DateTime date = r.FechaDesde.Date;
                    TimeSpan start = r.HoraDesde ?? TimeSpan.Zero;
                    TimeSpan end = r.HoraHasta ?? new TimeSpan(23, 59, 0);

                    return new ScheduleBlockedSlotDto
                    {
                        Id = r.Id.ToString(CultureInfo.InvariantCulture),
                        Date = date,
                        StartTime = start.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                        EndTime = end.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                        Reason = r.Motivo ?? string.Empty,
                        CreatedAt = r.FechaCreacion
                    };
                })
                .ToList();

            logger.LogDebug(
                "Se recuperaron {Count} bloqueos de agenda para profesional {ProfesionalId} entre {Desde} y {Hasta}",
                result.Count,
                idProf,
                from,
                to);

            return result;
        }

        public async Task<IEnumerable<ScheduleAvailableSlotDto>> GetAvailableSlotsAsync(string professionalUserId, DateTime date, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            int idProf = await ProfessionalResolver.ResolveRequiredAsync(con, professionalUserId, ct);
            int defaultDuration = await GetDefaultDurationAsync(con, idProf, ct);
            DateTime targetDate = date.Date;
            int dayOfWeek = (int)targetDate.DayOfWeek; // 0=Domingo,...6=Sábado

            const string sqlSlots = @"
                select
                    ""ID_AGENDA_PROFESIONALES"" as Id,
                    ""DIA_SEMANA"" as DiaSemana,
                    ""HORA_INICIO"" as HoraInicio,
                    ""HORA_FIN"" as HoraFin,
                    ""DURACION_MINUTOS"" as DuracionMinutos,
                    ""ES_LABORABLE"" as EsLaborable
                from public.""AGENDA_PROFESIONALES""
                where ""ID_PROFESIONALES"" = @idProf
                  and ""DIA_SEMANA"" = @dow
                  and ""ES_LABORABLE"" = TRUE
                order by ""HORA_INICIO"";";

            IEnumerable<AgendaRow> agendaRows = await con.QueryAsync<AgendaRow>(sqlSlots, new { idProf, dow = dayOfWeek });

            if (!agendaRows.Any())
            {
                logger.LogDebug(
                    "No se encontraron horarios configurados para profesional {ProfesionalId} en el día {DiaSemana}",
                    idProf,
                    dayOfWeek);
                return Array.Empty<ScheduleAvailableSlotDto>();
            }

            const string sqlTurnos = @"
                select
                    ""ID_TURNOS"" as Id,
                    ""HORA_INICIO"" as HoraInicio,
                    ""HORA_FIN"" as HoraFin
                from public.""TURNOS""
                where ""ID_PROFESIONALES"" = @idProf
                  and ""FECHA"" = @fecha;";

            IEnumerable<AppointmentSlotRow> appointments = await con.QueryAsync<AppointmentSlotRow>(sqlTurnos, new { idProf, fecha = targetDate });

            const string sqlAusencias = @"
                select
                    ""FECHA_DESDE"" as FechaDesde,
                    ""FECHA_HASTA"" as FechaHasta,
                    ""HORA_DESDE"" as HoraDesde,
                    ""HORA_HASTA"" as HoraHasta
                from public.""AUSENCIAS_PROFESIONALES""
                where ""ID_PROFESIONALES"" = @idProf
                  and @fecha between ""FECHA_DESDE"" and ""FECHA_HASTA"";";

            IEnumerable<AbsenceRow> absences = await con.QueryAsync<AbsenceRow>(sqlAusencias, new { idProf, fecha = targetDate });

            List<ScheduleAvailableSlotDto> result = new List<ScheduleAvailableSlotDto>();

            foreach (AgendaRow row in agendaRows)
            {
                int duration = row.DuracionMinutos ?? defaultDuration;
                if (duration <= 0)
                {
                    continue;
                }

                TimeSpan step = TimeSpan.FromMinutes(duration);
                TimeSpan start = row.HoraInicio;
                TimeSpan end = row.HoraFin;

                for (TimeSpan current = start; current + step <= end; current = current + step)
                {
                    TimeSpan slotStart = current;
                    TimeSpan slotEnd = current + step;

                    bool overlapsAbsence = absences.Any(a => Overlaps(slotStart, slotEnd, a.HoraDesde, a.HoraHasta));

                    AppointmentSlotRow? appointment = appointments.FirstOrDefault(t =>
                        Overlaps(slotStart, slotEnd, t.HoraInicio, t.HoraFin));

                    bool available = !overlapsAbsence && appointment is null;

                    ScheduleAvailableSlotDto dto = new ScheduleAvailableSlotDto
                    {
                        Date = targetDate,
                        Time = slotStart.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                        Available = available,
                        AppointmentId = appointment is null ? null : appointment.Id.ToString(CultureInfo.InvariantCulture),
                        Duration = duration
                    };

                    result.Add(dto);
                }
            }

            List<ScheduleAvailableSlotDto> ordered = result.OrderBy(r => r.Time, StringComparer.Ordinal).ToList();

            int disponibles = ordered.Count(s => s.Available);
            int ocupados = ordered.Count - disponibles;

            logger.LogDebug(
                "Cálculo de disponibilidad para profesional {ProfesionalId} en fecha {Fecha}: TotalSlots={Total}, Disponibles={Disponibles}, Ocupados={Ocupados}",
                idProf,
                targetDate,
                ordered.Count,
                disponibles,
                ocupados);

            return ordered;
        }

        private static async Task<int> GetDefaultDurationAsync(IDbConnection con, int idProf, CancellationToken ct)
        {
            const string sql = @"
                select ""DURACION_TURNO_EN_MINUTOS""
                from public.""PROFESIONALES""
                where ""ID_PROFESIONALES"" = @id
                limit 1;";

            int? value = await con.ExecuteScalarAsync<int?>(sql, new { id = idProf });
            return value.GetValueOrDefault(30);
        }

        private static ScheduleTimeSlotDto GetSlotDto(AgendaRow row, int defaultDuration)
        {
            int duration = row.DuracionMinutos ?? defaultDuration;

            return new ScheduleTimeSlotDto
            {
                Id = row.Id.ToString(CultureInfo.InvariantCulture),
                DayOfWeek = row.DiaSemana,
                StartTime = row.HoraInicio.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                EndTime = row.HoraFin.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                Duration = duration,
                IsActive = row.EsLaborable
            };
        }

        private static async Task<ScheduleTimeSlotDto> GetTimeSlotAsync(IDbConnection con, int idProf, int idSlot, CancellationToken ct)
        {
            const string sql = @"
                select
                    ""ID_AGENDA_PROFESIONALES"" as Id,
                    ""DIA_SEMANA"" as DiaSemana,
                    ""HORA_INICIO"" as HoraInicio,
                    ""HORA_FIN"" as HoraFin,
                    ""DURACION_MINUTOS"" as DuracionMinutos,
                    ""ES_LABORABLE"" as EsLaborable
                from public.""AGENDA_PROFESIONALES""
                where ""ID_AGENDA_PROFESIONALES"" = @idSlot
                  and ""ID_PROFESIONALES"" = @idProf
                limit 1;";

            AgendaRow? row = await con.QuerySingleOrDefaultAsync<AgendaRow>(sql, new { idSlot, idProf });
            if (row is null)
            {
                throw new NotFoundException("Horario no encontrado.");
            }

            int defaultDuration = await GetDefaultDurationAsync(con, idProf, ct);
            return GetSlotDto(row, defaultDuration);
        }

        private static TimeSpan ParseTime(string value)
        {
            if (TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan ts))
            {
                return ts;
            }

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out ts))
            {
                return ts;
            }

            throw new BadRequestException("Formato de hora inválido. Use HH:mm.");
        }

        private static bool Overlaps(TimeSpan start1, TimeSpan end1, TimeSpan? start2, TimeSpan? end2)
        {
            if (!start2.HasValue || !end2.HasValue)
            {
                return false;
            }

            TimeSpan s2 = start2.Value;
            TimeSpan e2 = end2.Value;

            return start1 < e2 && s2 < end1;
        }

        private sealed class AgendaRow
        {
            public int Id { get; set; }

            public int DiaSemana { get; set; }

            public TimeSpan HoraInicio { get; set; }

            public TimeSpan HoraFin { get; set; }

            public int? DuracionMinutos { get; set; }

            public bool EsLaborable { get; set; }
        }

        private sealed class AppointmentSlotRow
        {
            public int Id { get; set; }

            public TimeSpan HoraInicio { get; set; }

            public TimeSpan HoraFin { get; set; }
        }

        private sealed class AbsenceRow
        {
            public int Id { get; set; }

            public DateTime FechaDesde { get; set; }

            public DateTime FechaHasta { get; set; }

            public TimeSpan? HoraDesde { get; set; }

            public TimeSpan? HoraHasta { get; set; }

            public string? Motivo { get; set; }

            public DateTime FechaCreacion { get; set; }
        }
    }
}
