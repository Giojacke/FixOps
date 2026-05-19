using Mantenimiento.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mantenimiento.Infrastructure.Persistence.Repositories;

public class NotificacionRepository(MantenimientoDbContext context) : EfRepository<Notificacion>(context)
{
    public override async Task<IEnumerable<Notificacion>> GetAllAsync()
    {
        return await _context.Notificaciones
            .Include(n => n.OrdenTrabajo)
            .Include(n => n.Operacion)
            .ToListAsync();
    }
}
