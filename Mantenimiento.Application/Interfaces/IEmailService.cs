namespace Mantenimiento.Application.Interfaces;

public interface IEmailService
{
    // Domain-specific helpers (build body internally)
    Task SendSurveyAsync(string email, Guid ordenId);

    // Generic raw sender — used by EmailQueueWorker
    Task SendAsync(string to, string subject, string body);
}
