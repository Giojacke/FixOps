using Mantenimiento.Domain.Enums;

namespace Mantenimiento.Domain.Entities;

public class OrdenTrabajo
{
    public Guid Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaFinalizacion { get; set; }
    public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;
    public NivelUrgencia Urgencia { get; set; }

    public Guid SolicitanteId { get; set; }
    public Guid? TecnicoAsignadoId { get; set; }
    public Guid DependenciaId { get; set; }

    // Navigation properties
    public Usuario Solicitante { get; set; } = null!;
    public Usuario? TecnicoAsignado { get; set; }
    public Dependencia Dependencia { get; set; } = null!;
    public ICollection<Operacion> Operaciones { get; set; } = new List<Operacion>();
    public EncuestaSatisfaccion? Encuesta { get; set; }
}
