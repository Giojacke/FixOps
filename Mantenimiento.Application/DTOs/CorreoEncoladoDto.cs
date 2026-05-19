namespace Mantenimiento.Application.DTOs;

public class CorreoEncoladoDto
{
    public Guid      Id                 { get; set; }
    public string    Destinatario       { get; set; } = string.Empty;
    public string    Asunto             { get; set; } = string.Empty;
    public string    TipoCorreo         { get; set; } = string.Empty;
    public string    Estado             { get; set; } = string.Empty;
    public int       Intentos           { get; set; }
    public int       MaxIntentos        { get; set; }
    public DateTime  FechaCreacion      { get; set; }
    public DateTime? FechaUltimoIntento { get; set; }
    public DateTime? FechaEnvio         { get; set; }
    public DateTime? ProximoIntento     { get; set; }
    public string?   ErrorMensaje       { get; set; }
}

public class CorreoQueueFilter
{
    public string?   Estado     { get; set; }
    public string?   TipoCorreo { get; set; }
    public DateTime? Desde      { get; set; }
    public DateTime? Hasta      { get; set; }
    public int       Page       { get; set; } = 1;
    public int       PageSize   { get; set; } = 50;
}

public class CorreoQueueStatsDto
{
    public int Pendientes  { get; set; }
    public int Procesando  { get; set; }
    public int Enviados    { get; set; }
    public int Fallidos    { get; set; }
    public int Total       { get; set; }
}
