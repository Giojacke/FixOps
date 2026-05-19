using Mantenimiento.Domain.Enums;

namespace Mantenimiento.Application.DTOs;

public record OrdenTrabajoDto(
    Guid Id,
    string Folio,
    string Descripcion,
    DateTime FechaCreacion,
    DateTime? FechaFinalizacion,
    EstadoOrden Estado,
    NivelUrgencia Urgencia,
    Guid SolicitanteId,
    string SolicitanteNombre,
    Guid? TecnicoAsignadoId,
    string? TecnicoAsignadoNombre,
    Guid DependenciaId,
    string DependenciaNombre,
    IEnumerable<OperacionDto> Operaciones,
    bool TieneEncuesta = false
);
