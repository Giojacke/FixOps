using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Application.Interfaces;

public interface IEncuestaService
{
    Task<Result<Guid>> RegistrarEncuestaAsync(CrearEncuestaRequest request);
    Task<IEnumerable<EncuestaDto>> GetResultadosAsync(DateTime? desde = null, DateTime? hasta = null);
    Task<MetricasDto> GetMetricasAsync();
}
