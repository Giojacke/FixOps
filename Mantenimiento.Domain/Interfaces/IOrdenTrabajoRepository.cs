using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Enums;

namespace Mantenimiento.Domain.Interfaces;

public interface IOrdenTrabajoRepository : IRepository<OrdenTrabajo>
{
    Task<IEnumerable<OrdenTrabajo>> GetOrdenesByTecnicoIdAsync(Guid tecnicoId);
    Task<IEnumerable<OrdenTrabajo>> GetOrdenesByEstadoAsync(EstadoOrden estado);
    Task<IEnumerable<OrdenTrabajo>> GetOrdenesByDependenciaIdAsync(Guid dependenciaId);
    Task<string> GenerateFolioAsync();
    Task<IEnumerable<OrdenTrabajo>> GetFilteredAsync(
        EstadoOrden?   estado    = null,
        NivelUrgencia? urgencia  = null,
        DateTime?      desde     = null,
        DateTime?      hasta     = null,
        Guid?          id        = null,
        string?        folio     = null,
        Guid?          tecnicoId = null);
}
