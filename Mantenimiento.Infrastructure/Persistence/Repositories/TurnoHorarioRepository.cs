using Mantenimiento.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mantenimiento.Infrastructure.Persistence.Repositories;

public class TurnoHorarioRepository(MantenimientoDbContext context) : EfRepository<TurnoHorario>(context)
{
    public override async Task<IEnumerable<TurnoHorario>> GetAllAsync()
    {
        return await _context.Turnos
            .Include(t => t.Tecnico)
            .ToListAsync();
    }
}
