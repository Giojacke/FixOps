using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Mantenimiento.Application.Mappings;
using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Enums;
using Mantenimiento.Domain.Interfaces;

namespace Mantenimiento.Application.Services;

public class OrdenTrabajoService(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ICurrentUserService currentUser) : IOrdenTrabajoService
{
    public async Task<Result<IEnumerable<OrdenTrabajoDto>>> GetOrdenesByTecnicoIdAsync(Guid tecnicoId)
    {
        var ordenes = await unitOfWork.OrdenesTrabajo.GetOrdenesByTecnicoIdAsync(tecnicoId);
        return Result<IEnumerable<OrdenTrabajoDto>>.Success(ordenes.Select(o => o.ToDto()));
    }

    public async Task<Result> ActualizarEstadoOrdenAsync(Guid ordenId, EstadoOrden nuevoEstado)
    {
        var orden = await unitOfWork.OrdenesTrabajo.GetByIdAsync(ordenId);
        if (orden == null) return Result.Failure("Orden no encontrada.");

        if (nuevoEstado == EstadoOrden.Finalizada)
        {
            if (!orden.Operaciones.Any())
                return Result.Failure("No se puede finalizar una orden sin operaciones.");

            orden.Estado = EstadoOrden.Finalizada;
            orden.FechaFinalizacion = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();

            var jefeEmail = orden.Dependencia?.JefeEmail;
            if (!string.IsNullOrWhiteSpace(jefeEmail))
            {
                try { await emailService.SendSurveyAsync(jefeEmail, orden.Id); }
                catch { /* No bloquear la finalización si falla el email */ }
            }
        }
        else
        {
            orden.Estado = nuevoEstado;
            if (nuevoEstado == EstadoOrden.Cancelada)
                await RegistrarAuditoriaAsync(
                    "Orden", orden.Id,
                    $"Folio: {orden.Folio} – {orden.Descripcion}",
                    "Cancelacion");
            await unitOfWork.SaveChangesAsync();
        }

        return Result.Success();
    }

    public async Task<Result> ActualizarEstadoOperacionAsync(Guid operacionId, EstadoOperacion nuevoEstado, Guid tecnicoId)
    {
        var operacion = await unitOfWork.Operaciones.GetByIdAsync(operacionId);
        if (operacion == null) return Result.Failure("Operación no encontrada.");

        if (operacion.Estado == EstadoOperacion.Finalizada)
            return Result.Failure("No se puede modificar una operación ya finalizada.");

        var orden = await unitOfWork.OrdenesTrabajo.GetByIdAsync(operacion.OrdenTrabajoId);
        if (orden == null) return Result.Failure("Orden no encontrada.");

        if (orden.TecnicoAsignadoId != tecnicoId)
            return Result.Failure("Solo el técnico asignado puede actualizar el estado de la operación.");

        operacion.Estado = nuevoEstado;
        unitOfWork.Operaciones.Update(operacion);

        // Auto-transición de la orden según el estado de sus operaciones
        if (nuevoEstado == EstadoOperacion.EnProceso && orden.Estado == EstadoOrden.Pendiente)
        {
            orden.Estado = EstadoOrden.EnProceso;
        }
        else if (nuevoEstado == EstadoOperacion.Finalizada)
        {
            var todasFinalizadas = orden.Operaciones
                .Where(o => o.Id != operacionId)
                .All(o => o.Estado == EstadoOperacion.Finalizada);

            if (todasFinalizadas && orden.Operaciones.Any())
            {
                orden.Estado = EstadoOrden.Finalizada;
                orden.FechaFinalizacion = DateTime.UtcNow;
                var jefeEmail = orden.Dependencia?.JefeEmail;
                if (!string.IsNullOrWhiteSpace(jefeEmail))
                {
                    try { await emailService.SendSurveyAsync(jefeEmail, orden.Id); }
                    catch { }
                }
            }
        }

        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> EliminarOperacionAsync(Guid operacionId)
    {
        var operacion = await unitOfWork.Operaciones.GetByIdAsync(operacionId);
        if (operacion == null) return Result.Failure("Operación no encontrada.");

        if (operacion.Estado == EstadoOperacion.Finalizada)
            return Result.Failure("No se puede eliminar una operación ya finalizada.");

        var orden = await unitOfWork.OrdenesTrabajo.GetByIdAsync(operacion.OrdenTrabajoId);
        if (orden?.Estado == EstadoOrden.Finalizada || orden?.Estado == EstadoOrden.Cancelada)
            return Result.Failure("No se pueden eliminar operaciones de una orden finalizada o cancelada.");

        unitOfWork.Operaciones.Remove(operacion);
        await RegistrarAuditoriaAsync(
            "Operacion", operacion.Id,
            $"Op. {operacion.Numero} – {operacion.Descripcion}",
            "Eliminacion");
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> EditarOperacionAsync(Guid operacionId, int numero, string descripcion, int horasHombre)
    {
        var operacion = await unitOfWork.Operaciones.GetByIdAsync(operacionId);
        if (operacion == null) return Result.Failure("Operación no encontrada.");

        if (operacion.Estado == EstadoOperacion.Finalizada)
            return Result.Failure("No se puede editar una operación ya finalizada.");

        var orden = await unitOfWork.OrdenesTrabajo.GetByIdAsync(operacion.OrdenTrabajoId);
        if (orden?.Estado == EstadoOrden.Finalizada || orden?.Estado == EstadoOrden.Cancelada)
            return Result.Failure("No se pueden editar operaciones de una orden finalizada o cancelada.");

        if (operacion.Numero != numero && orden!.Operaciones.Any(o => o.Numero == numero && o.Id != operacionId))
            return Result.Failure($"La operación {numero} ya existe en esta orden. Seleccione un número diferente.");

        operacion.Numero = numero;
        operacion.Descripcion = descripcion;
        operacion.HorasHombre = horasHombre;
        unitOfWork.Operaciones.Update(operacion);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<OrdenTrabajoDto>> CrearOrdenAsync(CrearOrdenRequest request)
    {
        var folio = await unitOfWork.OrdenesTrabajo.GenerateFolioAsync();
        var entity = new Mantenimiento.Domain.Entities.OrdenTrabajo
        {
            Id = Guid.NewGuid(),
            Folio = folio,
            Descripcion = request.Descripcion,
            Urgencia = request.Urgencia,
            SolicitanteId = request.IdSolicitante,
            DependenciaId = request.IdDependencia,
            TecnicoAsignadoId = request.IdTecnicoAsignado,
            Estado = EstadoOrden.Pendiente
        };

        await unitOfWork.OrdenesTrabajo.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();

        return Result<OrdenTrabajoDto>.Success(entity.ToDto());
    }

    public async Task<Result> AgregarOperacionAsync(Guid ordenId, int numero, string descripcion, int horasHombre)
    {
        var orden = await unitOfWork.OrdenesTrabajo.GetByIdAsync(ordenId);
        if (orden == null) return Result.Failure("Orden no encontrada.");

        if (orden.Estado == EstadoOrden.Finalizada || orden.Estado == EstadoOrden.Cancelada)
            return Result.Failure($"No se pueden agregar operaciones a una orden en estado {orden.Estado}.");

        if (orden.Operaciones.Any(o => o.Numero == numero))
            return Result.Failure($"La operación {numero} ya existe en esta orden. Seleccione un número diferente.");

        var operacion = new Mantenimiento.Domain.Entities.Operacion
        {
            Id = Guid.NewGuid(),
            OrdenTrabajoId = ordenId,
            Numero = numero,
            Descripcion = descripcion,
            HorasHombre = horasHombre,
            Estado = EstadoOperacion.Pendiente,
            FechaRealizacion = DateTime.UtcNow
        };

        await unitOfWork.Operaciones.AddAsync(operacion);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<OrdenTrabajoDto>> GetByIdAsync(Guid id)
    {
        var orden = await unitOfWork.OrdenesTrabajo.GetByIdAsync(id);
        if (orden == null) return Result<OrdenTrabajoDto>.Failure("Orden no encontrada.");
        return Result<OrdenTrabajoDto>.Success(orden.ToDto());
    }

    public async Task<Result<IEnumerable<OrdenTrabajoDto>>> GetAllAsync()
    {
        var ordenes = await unitOfWork.OrdenesTrabajo.GetAllAsync();
        return Result<IEnumerable<OrdenTrabajoDto>>.Success(ordenes.Select(o => o.ToDto()));
    }

    public async Task<Result<IEnumerable<OrdenTrabajoDto>>> GetFilteredAsync(OrdenTrabajoFilter filter)
    {
        var ordenes = await unitOfWork.OrdenesTrabajo.GetFilteredAsync(
            filter.Estado, filter.Urgencia, filter.Desde, filter.Hasta,
            filter.Id, filter.Folio, filter.TecnicoId);
        return Result<IEnumerable<OrdenTrabajoDto>>.Success(ordenes.Select(o => o.ToDto()));
    }

    public async Task<Result> RegistrarActividadAsync(Guid operacionId, RegistrarActividadRequest request, Guid tecnicoId)
    {
        var operacion = await unitOfWork.Operaciones.GetByIdAsync(operacionId);
        if (operacion == null) return Result.Failure("Operación no encontrada.");

        var orden = await unitOfWork.OrdenesTrabajo.GetByIdAsync(operacion.OrdenTrabajoId);
        if (orden == null) return Result.Failure("Orden no encontrada.");

        if (orden.TecnicoAsignadoId != tecnicoId)
            return Result.Failure("Solo el técnico asignado puede registrar actividad en esta operación.");

        if (operacion.Estado == EstadoOperacion.Finalizada)
            return Result.Failure("No se puede registrar actividad en una operación ya finalizada.");

        operacion.FechaInicio  = request.FechaInicio;
        operacion.MotivoPausa  = request.MotivoPausa;

        if (request.EsFinalizacion)
        {
            operacion.FechaFin             = request.FechaFin;
            operacion.DetalleFinalizacion  = request.DetalleFinalizacion;
            operacion.Estado               = EstadoOperacion.Finalizada;

            if (orden.Estado == EstadoOrden.Pendiente)
                orden.Estado = EstadoOrden.EnProceso;

            unitOfWork.Operaciones.Update(operacion);
            await unitOfWork.SaveChangesAsync();

            var todasFinalizadas = orden.Operaciones
                .Where(o => o.Id != operacionId)
                .All(o => o.Estado == EstadoOperacion.Finalizada);

            if (todasFinalizadas && orden.Operaciones.Any())
            {
                orden.Estado             = EstadoOrden.Finalizada;
                orden.FechaFinalizacion  = DateTime.UtcNow;
                await unitOfWork.SaveChangesAsync();

                var jefeEmail = orden.Dependencia?.JefeEmail;
                if (!string.IsNullOrWhiteSpace(jefeEmail))
                {
                    try { await emailService.SendSurveyAsync(jefeEmail, orden.Id); }
                    catch { }
                }
            }
        }
        else
        {
            operacion.Estado              = EstadoOperacion.Pausa;
            operacion.FechaFin            = null;
            operacion.DetalleFinalizacion = null;

            if (orden.Estado == EstadoOrden.Pendiente)
                orden.Estado = EstadoOrden.EnProceso;

            unitOfWork.Operaciones.Update(operacion);

            foreach (var mat in request.MaterialesSolicitados)
            {
                await unitOfWork.SolicitudesMateriales.AddAsync(new Domain.Entities.SolicitudMaterial
                {
                    Id              = Guid.NewGuid(),
                    OperacionId     = operacionId,
                    MaterialId      = mat.MaterialId,
                    NombreMaterial  = mat.NombreMaterial,
                    Cantidad        = mat.Cantidad,
                    EsPersonalizado = mat.EsPersonalizado,
                    FechaSolicitud  = DateTime.UtcNow
                });
            }

            await unitOfWork.SaveChangesAsync();
        }

        return Result.Success();
    }

    private async Task RegistrarAuditoriaAsync(string tipoEntidad, Guid entidadId, string resumen, string accion)
    {
        await unitOfWork.Auditorias.AddAsync(new RegistroAuditoria
        {
            Id             = Guid.NewGuid(),
            TipoEntidad    = tipoEntidad,
            EntidadId      = entidadId,
            ResumenEntidad = resumen,
            Accion         = accion,
            FechaHora      = DateTime.UtcNow,
            UsuarioId      = currentUser.UserId,
            UsuarioNombre  = currentUser.UserName,
            UsuarioEmail   = currentUser.Email
        });
    }
}
