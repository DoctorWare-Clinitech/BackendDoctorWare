using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using DoctorWare.Constants;
using DoctorWare.Data.Interfaces;
using DoctorWare.Services.Interfaces;
using DoctorWare.Services.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DoctorWare.Services.Implementation
{
    public class AppointmentReminderService : IAppointmentReminderService
    {
        private readonly IDbConnectionFactory factory;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration configuration;
        private readonly ILogger<AppointmentReminderService> logger;

        public AppointmentReminderService(
            IDbConnectionFactory factory,
            IEmailSender emailSender,
            IConfiguration configuration,
            ILogger<AppointmentReminderService> logger)
        {
            this.factory = factory;
            this.emailSender = emailSender;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<int> SendPendingRemindersAsync(CancellationToken cancellationToken = default)
        {
            int windowHours = GetReminderWindowHours();
            DateTime nowUtc = DateTime.UtcNow;
            DateTime toUtc = nowUtc.AddHours(windowHours);

            using IDbConnection con = factory.CreateConnection();
            string sql = @"
                select
                    t.""ID_TURNOS"" as IdTurno,
                    t.""FECHA"" as Fecha,
                    t.""HORA_INICIO"" as HoraInicio,
                    t.""RECORDATORIO_ENVIADO"" as RecordatorioEnviado,
                    p.""NOMBRE"" as PacienteNombre,
                    p.""APELLIDO"" as PacienteApellido,
                    per.""EMAIL_PRINCIPAL"" as EmailPaciente
                from public.""TURNOS"" t
                join public.""PACIENTES"" pa on pa.""ID_PACIENTES"" = t.""ID_PACIENTES""
                join public.""PERSONAS"" p on p.""ID_PERSONAS"" = pa.""ID_PERSONAS""
                left join public.""PERSONAS"" per on per.""ID_PERSONAS"" = pa.""ID_PERSONAS""
                join public.""ESTADOS_TURNO"" et on et.""ID_ESTADOS_TURNO"" = t.""ID_ESTADOS_TURNO""
                where coalesce(t.""RECORDATORIO_ENVIADO"", false) = false
                  and et.""NOMBRE"" in ('Programado', 'Confirmado')
                  and (t.""FECHA"" + t.""HORA_INICIO"") between @from and @to
                  and per.""EMAIL_PRINCIPAL"" is not null;";

            DateTime fromLocal = nowUtc.ToLocalTime();
            DateTime toLocal = toUtc.ToLocalTime();

            IEnumerable<ReminderRow> rows = await con.QueryAsync<ReminderRow>(sql, new { from = fromLocal, to = toLocal });

            List<ReminderRow> reminders = rows.ToList();
            if (reminders.Count == 0)
            {
                logger.LogDebug("No hay recordatorios de turnos pendientes dentro de la ventana de {Hours} horas.", windowHours);
                return 0;
            }

            logger.LogInformation("Se encontraron {Count} turnos para enviar recordatorio.", reminders.Count);

            int sent = 0;
            List<int> sentIds = new List<int>();
            foreach (ReminderRow row in reminders)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                string? email = row.EmailPaciente;
                if (string.IsNullOrWhiteSpace(email))
                {
                    continue;
                }

                string fullName = string.Join(" ",
                    new[] { row.PacienteNombre ?? string.Empty, row.PacienteApellido ?? string.Empty }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

                DateTime date = row.Fecha.Date;
                string startTime = row.HoraInicio.ToString(@"hh\:mm");

                string subject = "Recordatorio de turno - DoctorWare";
                string body = EmailTemplates.BuildAppointmentReminderEmail(fullName, date, startTime);

                try
                {
                    await emailSender.SendEmailAsync(email, subject, body, cancellationToken);
                    sent++;
                    sentIds.Add(row.IdTurno);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error al enviar recordatorio de turno {TurnoId} a {Email}", row.IdTurno, email);
                    continue;
                }
            }

            if (sent > 0 && sentIds.Count > 0)
            {
                string updateSql = @"
                    update public.""TURNOS""
                       set ""RECORDATORIO_ENVIADO"" = true,
                           ""FECHA_RECORDATORIO"" = @now,
                           ""ULTIMA_ACTUALIZACION"" = coalesce(""ULTIMA_ACTUALIZACION"", @now)
                     where ""ID_TURNOS"" = any(@ids);";

                int[] ids = sentIds.ToArray();
                await con.ExecuteAsync(updateSql, new { ids, now = DateTime.UtcNow });
            }

            logger.LogInformation("Se enviaron {Count} recordatorios de turno.", sent);
            return sent;
        }

        private int GetReminderWindowHours()
        {
            IConfigurationSection section = configuration.GetSection("Appointments:Reminder");
            int configured = section.GetValue<int?>("HoursBefore") ?? 24;
            return configured > 0 ? configured : 24;
        }

        private sealed class ReminderRow
        {
            public int IdTurno { get; set; }

            public DateTime Fecha { get; set; }

            public TimeSpan HoraInicio { get; set; }

            public bool RecordatorioEnviado { get; set; }

            public string? PacienteNombre { get; set; }

            public string? PacienteApellido { get; set; }

            public string? EmailPaciente { get; set; }
        }
    }
}
