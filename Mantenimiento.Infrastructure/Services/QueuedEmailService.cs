using Mantenimiento.Application.Interfaces;
using Mantenimiento.Domain.Entities;

namespace Mantenimiento.Infrastructure.Services;

/// <summary>
/// Drop-in replacement for IEmailService that enqueues emails for async delivery
/// instead of sending them synchronously. The actual SMTP send is done by EmailQueueWorker.
/// </summary>
public class QueuedEmailService(ICorreoQueueService queueService) : IEmailService
{
    public async Task SendSurveyAsync(string email, Guid ordenId)
    {
        var surveyUrl = $"http://localhost:5166/encuesta/{ordenId}";
        var body = $"""
            <p>Estimado usuario,</p>
            <p>Su orden de trabajo ha sido <strong>finalizada</strong>.</p>
            <p>Por favor complete la encuesta de satisfacción:</p>
            <p><a href="{surveyUrl}">{surveyUrl}</a></p>
            """;
        await queueService.EnqueueAsync(email, "Encuesta de Satisfacción — FixOps", body, TiposCorreo.Survey);
    }

    public async Task SendAsync(string to, string subject, string body)
        => await queueService.EnqueueAsync(to, subject, body, TiposCorreo.General);
}
