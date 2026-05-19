using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Enums;
using Mantenimiento.Domain.Interfaces;

namespace Mantenimiento.Application.Services;

public class EncuestaService(IUnitOfWork unitOfWork) : IEncuestaService
{
    public async Task<Result<Guid>> RegistrarEncuestaAsync(CrearEncuestaRequest request)
    {
        var orden = await unitOfWork.OrdenesTrabajo.GetByIdAsync(request.OrdenTrabajoId);
        if (orden == null) return Result<Guid>.Failure("Orden no encontrada.");
        if (orden.Estado != EstadoOrden.Finalizada) return Result<Guid>.Failure("Solo se pueden registrar encuestas para órdenes finalizadas.");
        if (orden.Encuesta != null) return Result<Guid>.Failure("Esta orden ya tiene una encuesta registrada.");

        var encuesta = new EncuestaSatisfaccion
        {
            Id = Guid.NewGuid(),
            OrdenTrabajoId = request.OrdenTrabajoId,
            PuntajeAtencion = request.PuntajeAtencion,
            PuntajeServicio = request.PuntajeServicio,
            PuntajeTiempo = request.PuntajeTiempo,
            Comentarios = request.Comentarios,
            FechaRespuesta = DateTime.UtcNow
        };

        await unitOfWork.Encuestas.AddAsync(encuesta);
        await unitOfWork.SaveChangesAsync();

        return Result<Guid>.Success(encuesta.Id);
    }

    public async Task<IEnumerable<EncuestaDto>> GetResultadosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        var encuestas = await unitOfWork.Encuestas.GetAllWithOrdenAsync();

        if (desde.HasValue)
            encuestas = encuestas.Where(e => e.FechaRespuesta >= desde.Value);
        if (hasta.HasValue)
            encuestas = encuestas.Where(e => e.FechaRespuesta <= hasta.Value.AddDays(1));

        return encuestas.Select(e => new EncuestaDto(
            e.Id,
            e.PuntajeAtencion,
            e.PuntajeServicio,
            e.PuntajeTiempo,
            e.Comentarios,
            e.FechaRespuesta,
            e.OrdenTrabajoId,
            e.OrdenTrabajo?.Folio,
            e.OrdenTrabajo?.Dependencia?.Departamento
        ));
    }

    public async Task<MetricasDto> GetMetricasAsync()
    {
        var encuestas = (await unitOfWork.Encuestas.GetAllWithOrdenAsync()).ToList();

        var porTecnico = encuestas
            .Where(e => e.OrdenTrabajo?.TecnicoAsignado != null)
            .GroupBy(e => e.OrdenTrabajo.TecnicoAsignado!.Nombre)
            .Select(g => new MetricaTecnicoDto(
                NombreTecnico:  g.Key,
                Promedio:       Math.Round(g.Average(e => (e.PuntajeAtencion + e.PuntajeServicio + e.PuntajeTiempo) / 3.0), 2),
                TotalEncuestas: g.Count()
            ));

        var porDependencia = encuestas
            .Where(e => e.OrdenTrabajo?.Dependencia != null)
            .GroupBy(e => e.OrdenTrabajo.Dependencia!.Nombre)
            .Select(g => new MetricaDependenciaDto(
                Dependencia:    g.Key,
                Promedio:       Math.Round(g.Average(e => (e.PuntajeAtencion + e.PuntajeServicio + e.PuntajeTiempo) / 3.0), 2),
                TotalEncuestas: g.Count()
            ));

        return new MetricasDto(porTecnico, porDependencia);
    }
}
