namespace Mantenimiento.Domain.Entities;

public class Notificacion
{
    public Guid Id { get; set; }
    public Guid DestinatarioId { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public Guid? OrdenTrabajoId { get; set; }
    public Guid? OperacionId { get; set; }
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }

    public Usuario Destinatario { get; set; } = null!;
    public OrdenTrabajo? OrdenTrabajo { get; set; }
    public Operacion? Operacion { get; set; }
}
