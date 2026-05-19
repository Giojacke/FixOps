using Mantenimiento.Infrastructure.Persistence;
using Mantenimiento.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mantenimiento.Infrastructure.BackgroundServices;

public class EmailQueueWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<EmailQueueWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EmailQueueWorker iniciado. Intervalo: {Interval}s", Interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado en EmailQueueWorker");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync()
    {
        using var scope = scopeFactory.CreateScope();

        var queueService = scope.ServiceProvider.GetRequiredService<CorreoEncoladoService>();
        var smtpService  = scope.ServiceProvider.GetRequiredService<SmtpEmailService>();

        var pendientes = await queueService.GetPendientesParaEnviarAsync();
        if (pendientes.Count == 0) return;

        logger.LogInformation("EmailQueueWorker: procesando {Count} correo(s)", pendientes.Count);

        foreach (var correo in pendientes)
        {
            await queueService.MarcarProcesandoAsync(correo);

            try
            {
                await smtpService.SendAsync(correo.Destinatario, correo.Asunto, correo.Cuerpo);
                await queueService.MarcarEnviadoAsync(correo);
                logger.LogInformation("Correo enviado → {To} | Asunto: {Subject}", correo.Destinatario, correo.Asunto);
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                await queueService.MarcarFallidoOReintentarAsync(correo, msg);
                logger.LogWarning("Fallo al enviar a {To} (intento {N}/{Max}): {Error}",
                    correo.Destinatario, correo.Intentos, correo.MaxIntentos, msg);
            }
        }
    }
}
