using System;
using System.Threading;
using System.Threading.Tasks;
using DoctorWare.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DoctorWare.Services.Implementation
{
    public class AppointmentReminderWorker : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<AppointmentReminderWorker> logger;
        private readonly TimeSpan interval;

        public AppointmentReminderWorker(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<AppointmentReminderWorker> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;

            int minutes = configuration.GetValue<int?>("Appointments:Reminder:IntervalMinutes") ?? 30;
            if (minutes <= 0)
            {
                minutes = 30;
            }
            interval = TimeSpan.FromMinutes(minutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("AppointmentReminderWorker iniciado con intervalo de {Minutes} minutos.", interval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    IAppointmentReminderService reminderService = scope.ServiceProvider.GetRequiredService<IAppointmentReminderService>();

                    int sent = await reminderService.SendPendingRemindersAsync(stoppingToken);
                    if (sent > 0)
                    {
                        logger.LogInformation("AppointmentReminderWorker envió {Count} recordatorios.", sent);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error en AppointmentReminderWorker.");
                }

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            logger.LogInformation("AppointmentReminderWorker detenido.");
        }
    }
}
