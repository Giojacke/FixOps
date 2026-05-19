using Mantenimiento.Domain.Enums;

namespace Mantenimiento.Application.DTOs;

public class OrdenTrabajoFilter
{
    public EstadoOrden?   Estado    { get; set; }
    public NivelUrgencia? Urgencia  { get; set; }
    public DateTime?      Desde     { get; set; }
    public DateTime?      Hasta     { get; set; }
    public Guid?          Id        { get; set; }
    public string?        Folio     { get; set; }
    public Guid?          TecnicoId { get; set; }
    public int            Page      { get; set; } = 1;
    public int            PageSize  { get; set; } = 20;
}
