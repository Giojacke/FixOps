namespace Mantenimiento.Application.DTOs;

public class ConfiguracionCorreoDto
{
    public Guid   Id           { get; set; }
    public string SmtpHost     { get; set; } = string.Empty;
    public int    SmtpPort     { get; set; } = 587;
    public string SmtpUser     { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail    { get; set; } = string.Empty;
    public string FromName     { get; set; } = "FixOps";
    public bool   EnableSsl    { get; set; } = true;
}

public class PruebaCorreoRequest
{
    public string EmailDestino { get; set; } = string.Empty;
}
