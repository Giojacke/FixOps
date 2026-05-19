using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Domain.Enums;

namespace Mantenimiento.Application.Interfaces;

public interface IOrdenTrabajoService
{
    Task<Result<IEnumerable<OrdenTrabajoDto>>> GetOrdenesByTecnicoIdAsync(Guid tecnicoId);
    Task<Result> ActualizarEstadoOrdenAsync(Guid ordenId, EstadoOrden nuevoEstado);
    Task<Result> ActualizarEstadoOperacionAsync(Guid operacionId, EstadoOperacion nuevoEstado, Guid tecnicoId);
    Task<Result<OrdenTrabajoDto>> CrearOrdenAsync(CrearOrdenRequest request);
    Task<Result> AgregarOperacionAsync(Guid ordenId, int numero, string descripcion, int horasHombre);
    Task<Result> EliminarOperacionAsync(Guid operacionId);
    Task<Result> EditarOperacionAsync(Guid operacionId, int numero, string descripcion, int horasHombre);
    Task<Result<OrdenTrabajoDto>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<OrdenTrabajoDto>>> GetAllAsync();
    Task<Result<IEnumerable<OrdenTrabajoDto>>> GetFilteredAsync(OrdenTrabajoFilter filter);
    Task<Result> RegistrarActividadAsync(Guid operacionId, RegistrarActividadRequest request, Guid tecnicoId);
}
