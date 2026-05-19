namespace Mantenimiento.Application.DTOs;

public class RegistroAuditoriaDto
{
    public Guid     Id             { get; set; }
    public string   TipoEntidad    { get; set; } = string.Empty;
    public Guid     EntidadId      { get; set; }
    public string   ResumenEntidad { get; set; } = string.Empty;
    public string   Accion         { get; set; } = string.Empty;
    public DateTime FechaHora      { get; set; }
    public Guid?    UsuarioId      { get; set; }
    public string   UsuarioNombre  { get; set; } = string.Empty;
    public string   UsuarioEmail   { get; set; } = string.Empty;
}

public class AuditoriaFilter
{
    public DateTime? FechaDesde  { get; set; }
    public DateTime? FechaHasta  { get; set; }
    public string?   TipoEntidad { get; set; }
    public string?   UsuarioEmail { get; set; }
    public int Page     { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

// ── Importación de horarios ─────────────────────────────────────────────────

public class ImportarHorariosRequest
{
    public List<HorarioImportItem> Items { get; set; } = [];
}

public class HorarioImportItem
{
    public string   EmailTecnico   { get; set; } = string.Empty;
    public DateOnly Fecha          { get; set; }
    public TimeOnly HoraInicio     { get; set; }
    public TimeOnly HoraFin        { get; set; }
    public bool     IncluyeAlmuerzo { get; set; }
}

public class ImportarHorariosResult
{
    public int          TotalRecibidos { get; set; }
    public int          Importados     { get; set; }
    public List<string> Errores        { get; set; } = [];
}
