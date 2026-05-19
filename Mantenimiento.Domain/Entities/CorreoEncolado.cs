namespace Mantenimiento.Domain.Entities;

public class CorreoEncolado
{
    public Guid      Id                  { get; set; }
    public string    Destinatario        { get; set; } = string.Empty;
    public string    Asunto              { get; set; } = string.Empty;
    public string    Cuerpo              { get; set; } = string.Empty;
    public string    TipoCorreo          { get; set; } = TiposCorreo.General;
    public string    Estado              { get; set; } = EstadosCorreo.Pendiente;
    public int       Intentos            { get; set; } = 0;
    public int       MaxIntentos         { get; set; } = 3;
    public DateTime  FechaCreacion       { get; set; }
    public DateTime? FechaUltimoIntento  { get; set; }
    public DateTime? FechaEnvio          { get; set; }
    public DateTime? ProximoIntento      { get; set; }
    public string?   ErrorMensaje        { get; set; }
}

public static class EstadosCorreo
{
    public const string Pendiente  = "Pendiente";
    public const string Procesando = "Procesando";
    public const string Enviado    = "Enviado";
    public const string Fallido    = "Fallido";
}

public static class TiposCorreo
{
    public const string Survey       = "Encuesta";
    public const string Notificacion = "Notificacion";
    public const string Prueba       = "Prueba";
    public const string General      = "General";
}
