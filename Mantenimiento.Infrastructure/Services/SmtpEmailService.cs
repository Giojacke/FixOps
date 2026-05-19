using Mantenimiento.Application.Interfaces;
using Mantenimiento.Domain.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Mantenimiento.Infrastructure.Services;

public class SmtpEmailService(IUnitOfWork unitOfWork) : IEmailService
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
        await SendAsync(email, "Encuesta de Satisfacción — FixOps", body);
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var conf = await GetConfigAsync();
        if (conf == null)
            throw new InvalidOperationException("Configuración SMTP no disponible. Configure el servidor en Configuración → Correo.");

        using var client = new SmtpClient(conf.SmtpHost, conf.SmtpPort)
        {
            Credentials = new NetworkCredential(conf.SmtpUser, conf.SmtpPassword),
            EnableSsl   = conf.EnableSsl
        };

        var from = string.IsNullOrWhiteSpace(conf.FromEmail) ? conf.SmtpUser : conf.FromEmail;
        var mail = new MailMessage
        {
            From       = new MailAddress(from, conf.FromName),
            Subject    = subject,
            Body       = body,
            IsBodyHtml = true
        };
        mail.To.Add(to);

        await client.SendMailAsync(mail);
    }

    private async Task<Domain.Entities.ConfiguracionCorreo?> GetConfigAsync()
    {
        var all = await unitOfWork.ConfiguracionCorreo.GetAllAsync();
        var c   = all.FirstOrDefault();
        return c is { SmtpHost.Length: > 0, SmtpUser.Length: > 0 } ? c : null;
    }
}
