namespace Mantenimiento.Application.DTOs;

public class NotificacionDto
{
    public Guid   Id              { get; set; }
    public string Mensaje         { get; set; } = string.Empty;
    public Guid?  OrdenTrabajoId  { get; set; }
    public string FolioOrden      { get; set; } = string.Empty;
    public Guid?  OperacionId     { get; set; }
    public int    NumeroOperacion { get; set; }
    public bool   Leida           { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class TurnoHorarioDto
{
    public Guid     Id              { get; set; }
    public Guid     TecnicoId       { get; set; }
    public string   TecnicoNombre   { get; set; } = string.Empty;
    public DateOnly Fecha           { get; set; }
    public TimeOnly HoraInicio      { get; set; }
    public TimeOnly HoraFin         { get; set; }
    public bool     IncluyeAlmuerzo { get; set; }
    public double   HorasEfectivas  { get; set; }
}

public class GuardarTurnosRequest
{
    public Guid TecnicoId { get; set; }
    public List<TurnoHorarioDto> Turnos { get; set; } = [];
}

public class SugerirHorarioRequest
{
    public Guid      TecnicoId    { get; set; }
    public DateOnly  FechaInicio  { get; set; }
    public string    Periodo      { get; set; } = "Semanal"; // Semanal | Quincenal | Mensual
}

public class ConfiguracionEmpresaDto
{
    public Guid     Id                      { get; set; }
    public bool     AlmuerzoPagadoEnJornada { get; set; }
    public int      HorasDiariasEfectivas   { get; set; }
    public int      HorasSemanalesMaximas   { get; set; }
    public int      HorasExtrasMaximas      { get; set; }
    public TimeOnly HoraInicioDefault       { get; set; }
}

public class RecomendacionHorarioDto
{
    public Guid     Id         { get; set; }
    public string   Nombre     { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin    { get; set; }
    public bool     Activo     { get; set; }
}

public class AprobacionMaterialRequest
{
    public Guid   OperacionId    { get; set; }
    public Guid   TecnicoId      { get; set; }
    public string MensajeAdicional { get; set; } = string.Empty;
}
