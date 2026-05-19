using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Application.Interfaces;

public interface ICorreoQueueService
{
    // Enqueue a new email — returns the new record Id
    Task<Guid> EnqueueAsync(string destinatario, string asunto, string cuerpo, string tipo = "General");

    // Query
    Task<IEnumerable<CorreoEncoladoDto>> GetAllAsync(CorreoQueueFilter filter);
    Task<CorreoEncoladoDto?>             GetByIdAsync(Guid id);
    Task<CorreoQueueStatsDto>            GetStatsAsync();

    // Manual retry for a specific email (resets to Pendiente)
    Task<Result> ReintentarAsync(Guid id);

    // Retry all Fallido emails
    Task<int> ReintentarFallidosAsync();
}
