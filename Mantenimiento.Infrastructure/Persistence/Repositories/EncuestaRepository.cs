using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mantenimiento.Infrastructure.Persistence.Repositories;

public class EncuestaRepository : EfRepository<EncuestaSatisfaccion>, IEncuestaRepository
{
    public EncuestaRepository(MantenimientoDbContext context) : base(context) { }

    public async Task<IEnumerable<EncuestaSatisfaccion>> GetAllWithOrdenAsync()
    {
        return await _context.Set<EncuestaSatisfaccion>()
            .Include(e => e.OrdenTrabajo)
                .ThenInclude(o => o.Dependencia)
            .Include(e => e.OrdenTrabajo)
                .ThenInclude(o => o.TecnicoAsignado)
            .ToListAsync();
    }
}
