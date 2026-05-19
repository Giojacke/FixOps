using Mantenimiento.Domain.Enums;

namespace Mantenimiento.Application.DTOs;

public class RegistrarActividadRequest
{
    public bool EsFinalizacion   { get; set; }
    public MotivoPausa? MotivoPausa { get; set; }
    public DateTime FechaInicio  { get; set; }
    public DateTime? FechaFin    { get; set; }
    public string? DetalleFinalizacion { get; set; }
    public List<ItemSolicitudMaterial> MaterialesSolicitados { get; set; } = [];
}

public class ItemSolicitudMaterial
{
    public Guid? MaterialId      { get; set; }
    public string NombreMaterial { get; set; } = string.Empty;
    public int Cantidad          { get; set; }
    public bool EsPersonalizado  { get; set; }
}
