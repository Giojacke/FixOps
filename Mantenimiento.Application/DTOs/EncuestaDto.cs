namespace Mantenimiento.Application.DTOs;

public class EncuestaDto
{
    public Guid Id { get; set; }
    public int PuntajeAtencion { get; set; }
    public int PuntajeServicio { get; set; }
    public int PuntajeTiempo { get; set; }
    public string Comentarios { get; set; } = string.Empty;
    public DateTime FechaRespuesta { get; set; }
    public Guid OrdenTrabajoId { get; set; }
    public string? FolioOrden { get; set; }
    public string? Departamento { get; set; }

    public EncuestaDto() { }

    public EncuestaDto(Guid id, int puntajeAtencion, int puntajeServicio, int puntajeTiempo, string comentarios, DateTime fechaRespuesta, Guid ordenTrabajoId, string? folioOrden, string? departamento)
    {
        Id = id;
        PuntajeAtencion = puntajeAtencion;
        PuntajeServicio = puntajeServicio;
        PuntajeTiempo = puntajeTiempo;
        Comentarios = comentarios;
        FechaRespuesta = fechaRespuesta;
        OrdenTrabajoId = ordenTrabajoId;
        FolioOrden = folioOrden;
        Departamento = departamento;
    }
}

public class CrearEncuestaRequest
{
    public Guid OrdenTrabajoId { get; set; }
    public int PuntajeAtencion { get; set; }
    public int PuntajeServicio { get; set; }
    public int PuntajeTiempo { get; set; }
    public string Comentarios { get; set; } = string.Empty;

    public CrearEncuestaRequest() { }

    public CrearEncuestaRequest(Guid ordenTrabajoId, int puntajeAtencion, int puntajeServicio, int puntajeTiempo, string comentarios)
    {
        OrdenTrabajoId = ordenTrabajoId;
        PuntajeAtencion = puntajeAtencion;
        PuntajeServicio = puntajeServicio;
        PuntajeTiempo = puntajeTiempo;
        Comentarios = comentarios;
    }
}
