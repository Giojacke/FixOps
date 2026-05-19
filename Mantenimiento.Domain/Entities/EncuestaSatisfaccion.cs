namespace Mantenimiento.Domain.Entities;

public class EncuestaSatisfaccion
{
    public Guid Id { get; set; }
    public int PuntajeAtencion { get; set; } // 1-5
    public int PuntajeServicio { get; set; } // 1-5
    public int PuntajeTiempo { get; set; } // 1-5
    public string Comentarios { get; set; } = string.Empty;
    public DateTime FechaRespuesta { get; set; } = DateTime.UtcNow;

    public Guid OrdenTrabajoId { get; set; }
    
    // Navigation properties
    public OrdenTrabajo OrdenTrabajo { get; set; } = null!;
}
