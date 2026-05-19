using Mantenimiento.Domain.Enums;

namespace Mantenimiento.Domain.Entities;

public class Operacion
{
    public Guid Id                     { get; set; }
    public string Descripcion          { get; set; } = string.Empty;
    public DateTime FechaRealizacion   { get; set; } = DateTime.UtcNow;
    public int HorasHombre             { get; set; }
    public EstadoOperacion Estado      { get; set; } = EstadoOperacion.Pendiente;
    public int Numero                  { get; set; }

    public DateTime? FechaInicio       { get; set; }
    public DateTime? FechaFin          { get; set; }
    public MotivoPausa? MotivoPausa    { get; set; }
    public string? DetalleFinalizacion { get; set; }

    public Guid OrdenTrabajoId         { get; set; }

    public OrdenTrabajo OrdenTrabajo                              { get; set; } = null!;
    public ICollection<Material> MaterialesUsados                 { get; set; } = new List<Material>();
    public ICollection<SolicitudMaterial> SolicitudesMateriales   { get; set; } = new List<SolicitudMaterial>();
}
