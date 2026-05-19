using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Mantenimiento.Infrastructure.Services;

public class ConfiguracionCorreoService(IUnitOfWork unitOfWork) : IConfiguracionCorreoService
{
    public async Task<ConfiguracionCorreoDto> GetAsync()
    {
        var all  = await unitOfWork.ConfiguracionCorreo.GetAllAsync();
        var conf = all.FirstOrDefault();
        if (conf == null) return new ConfiguracionCorreoDto();
        return Map(conf);
    }

    public async Task<Result> UpdateAsync(ConfiguracionCorreoDto dto)
    {
        var all  = await unitOfWork.ConfiguracionCorreo.GetAllAsync();
        var conf = all.FirstOrDefault();

        if (conf == null)
        {
            conf = new ConfiguracionCorreo { Id = Guid.NewGuid() };
            await unitOfWork.ConfiguracionCorreo.AddAsync(conf);
        }

        conf.SmtpHost     = dto.SmtpHost.Trim();
        conf.SmtpPort     = dto.SmtpPort;
        conf.SmtpUser     = dto.SmtpUser.Trim();
        conf.FromEmail    = dto.FromEmail.Trim();
        conf.FromName     = dto.FromName.Trim();
        conf.EnableSsl    = dto.EnableSsl;

        // Only update password if a new value was provided
        if (!string.IsNullOrWhiteSpace(dto.SmtpPassword))
            conf.SmtpPassword = dto.SmtpPassword;

        unitOfWork.ConfiguracionCorreo.Update(conf);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> EnviarPruebaAsync(string emailDestino)
    {
        if (string.IsNullOrWhiteSpace(emailDestino))
            return Result.Failure("El email de destino es requerido.");

        var all  = await unitOfWork.ConfiguracionCorreo.GetAllAsync();
        var conf = all.FirstOrDefault();

        if (conf == null || string.IsNullOrWhiteSpace(conf.SmtpHost))
            return Result.Failure("La configuración SMTP no está completa. Guarde los datos primero.");

        try
        {
            using var client = new SmtpClient(conf.SmtpHost, conf.SmtpPort)
            {
                Credentials = new NetworkCredential(conf.SmtpUser, conf.SmtpPassword),
                EnableSsl   = conf.EnableSsl
            };

            var from = string.IsNullOrWhiteSpace(conf.FromEmail) ? conf.SmtpUser : conf.FromEmail;
            var mail = new MailMessage
            {
                From       = new MailAddress(from, conf.FromName),
                Subject    = "✅ Prueba de Configuración SMTP — FixOps",
                Body       = "<p>Este es un correo de prueba enviado desde <strong>FixOps</strong>.</p><p>Si recibes este mensaje, la configuración SMTP es correcta.</p>",
                IsBodyHtml = true
            };
            mail.To.Add(emailDestino);

            await client.SendMailAsync(mail);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error al enviar: {ex.Message}");
        }
    }

    private static ConfiguracionCorreoDto Map(ConfiguracionCorreo e) => new()
    {
        Id          = e.Id,
        SmtpHost    = e.SmtpHost,
        SmtpPort    = e.SmtpPort,
        SmtpUser    = e.SmtpUser,
        SmtpPassword = string.Empty, // never send password back to client
        FromEmail   = e.FromEmail,
        FromName    = e.FromName,
        EnableSsl   = e.EnableSsl
    };
}
