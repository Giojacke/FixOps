using Mantenimiento.Domain.Enums;

namespace Mantenimiento.Application.DTOs;

public record CrearOrdenRequest
{
    public string Descripcion { get; set; } = string.Empty;
    public NivelUrgencia Urgencia { get; set; }
    public Guid IdSolicitante { get; set; }
    public Guid IdDependencia { get; set; }
    public Guid? IdTecnicoAsignado { get; set; }
}
