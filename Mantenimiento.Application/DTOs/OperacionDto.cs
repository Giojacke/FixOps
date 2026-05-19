using Mantenimiento.Domain.Enums;

namespace Mantenimiento.Application.DTOs;

public record OperacionDto(
    Guid Id,
    int Numero,
    string Descripcion,
    DateTime FechaRealizacion,
    int HorasHombre,
    EstadoOperacion Estado,
    DateTime? FechaInicio = null,
    DateTime? FechaFin = null,
    MotivoPausa? MotivoPausa = null,
    string? DetalleFinalizacion = null,
    IEnumerable<SolicitudMaterialDto>? SolicitudesMateriales = null
);
