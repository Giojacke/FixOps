using Mantenimiento.Application.DTOs;
using Mantenimiento.Domain.Entities;

namespace Mantenimiento.Application.Mappings;

public static class MappingExtensions
{
    public static OrdenTrabajoDto ToDto(this OrdenTrabajo entity) => new(
        entity.Id,
        entity.Folio,
        entity.Descripcion,
        entity.FechaCreacion,
        entity.FechaFinalizacion,
        entity.Estado,
        entity.Urgencia,
        entity.SolicitanteId,
        entity.Solicitante?.Nombre ?? "N/A",
        entity.TecnicoAsignadoId,
        entity.TecnicoAsignado?.Nombre ?? "Sin asignar",
        entity.DependenciaId,
        entity.Dependencia?.Nombre ?? "N/A",
        entity.Operaciones.Select(o => o.ToDto()),
        entity.Encuesta != null
    );

    public static OperacionDto ToDto(this Operacion entity) => new(
        entity.Id,
        entity.Numero,
        entity.Descripcion,
        entity.FechaRealizacion,
        entity.HorasHombre,
        entity.Estado,
        entity.FechaInicio,
        entity.FechaFin,
        entity.MotivoPausa,
        entity.DetalleFinalizacion,
        entity.SolicitudesMateriales.Select(s => new SolicitudMaterialDto
        {
            Id              = s.Id,
            NombreMaterial  = s.NombreMaterial,
            Cantidad        = s.Cantidad,
            EsPersonalizado = s.EsPersonalizado,
            FechaSolicitud  = s.FechaSolicitud
        })
    );

    public static DependenciaDto ToDto(this Dependencia entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Ubicacion = entity.Ubicacion,
        Codigo = entity.Codigo,
        Regional = entity.Regional,
        Departamento = entity.Departamento,
        JefeEmail      = entity.JefeEmail,
        NombreContacto = entity.NombreContacto
    };

    public static MaterialDto ToDto(this Material entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        TipoMaterial = entity.TipoMaterial,
        Descripcion = entity.Descripcion,
        PrecioUnitario = entity.PrecioUnitario,
        StockActual = entity.StockActual
    };

    public static UsuarioDto ToDto(this Usuario entity) => new()
    {
        Id                = entity.Id,
        Nombre            = entity.Nombre,
        Email             = entity.Email,
        Rol               = entity.Rol.ToString(),
        DependenciaId     = entity.DependenciaId,
        DependenciaNombre = entity.Dependencia?.Nombre ?? "N/A",
        ProgramadorId     = entity.ProgramadorId,
        ProgramadorNombre = entity.Programador?.Nombre ?? string.Empty
    };
}
