using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Enums;
using Mantenimiento.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mantenimiento.Infrastructure.Persistence.Repositories;

public class OrdenTrabajoRepository : EfRepository<OrdenTrabajo>, IOrdenTrabajoRepository
{
    public OrdenTrabajoRepository(MantenimientoDbContext context) : base(context) { }

    public async Task<IEnumerable<OrdenTrabajo>> GetOrdenesByTecnicoIdAsync(Guid tecnicoId)
    {
        return await _context.OrdenesTrabajo
            .Include(o => o.Operaciones)
            .Include(o => o.Dependencia)
            .Include(o => o.Solicitante)
            .Where(o => o.TecnicoAsignadoId == tecnicoId)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrdenTrabajo>> GetOrdenesByEstadoAsync(EstadoOrden estado)
    {
        return await _context.OrdenesTrabajo
            .Include(o => o.Dependencia)
            .Where(o => o.Estado == estado)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrdenTrabajo>> GetOrdenesByDependenciaIdAsync(Guid dependenciaId)
    {
        return await _context.OrdenesTrabajo
            .Where(o => o.DependenciaId == dependenciaId)
            .ToListAsync();
    }

    public async Task<string> GenerateFolioAsync()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.OrdenesTrabajo.CountAsync(o => o.Folio.Contains(date)) + 1;
        return $"OT-{date}-{count:D3}";
    }

    public async Task<IEnumerable<OrdenTrabajo>> GetFilteredAsync(
        EstadoOrden?   estado    = null,
        NivelUrgencia? urgencia  = null,
        DateTime?      desde     = null,
        DateTime?      hasta     = null,
        Guid?          id        = null,
        string?        folio     = null,
        Guid?          tecnicoId = null)
    {
        var query = _context.OrdenesTrabajo
            .Include(o => o.Solicitante)
            .Include(o => o.TecnicoAsignado)
            .Include(o => o.Dependencia)
            .Include(o => o.Operaciones)
                .ThenInclude(op => op.SolicitudesMateriales)
            .Include(o => o.Encuesta)
            .AsQueryable();

        if (id.HasValue)
            return await query.Where(o => o.Id == id.Value).ToListAsync();

        if (estado.HasValue)    query = query.Where(o => o.Estado    == estado.Value);
        if (urgencia.HasValue)  query = query.Where(o => o.Urgencia  == urgencia.Value);
        if (tecnicoId.HasValue) query = query.Where(o => o.TecnicoAsignadoId == tecnicoId.Value);
        if (desde.HasValue)     query = query.Where(o => o.FechaCreacion >= desde.Value);
        if (hasta.HasValue)     query = query.Where(o => o.FechaCreacion <= hasta.Value.Date.AddDays(1).AddTicks(-1));
        if (!string.IsNullOrWhiteSpace(folio))
            query = query.Where(o => o.Folio.Contains(folio));

        return await query.OrderByDescending(o => o.FechaCreacion).ToListAsync();
    }

    public override async Task<IEnumerable<OrdenTrabajo>> GetAllAsync()
    {
        return await _context.OrdenesTrabajo
            .Include(o => o.Solicitante)
            .Include(o => o.TecnicoAsignado)
            .Include(o => o.Dependencia)
            .Include(o => o.Operaciones)
            .ToListAsync();
    }

    public override async Task<OrdenTrabajo?> GetByIdAsync(Guid id)
    {
        return await _context.OrdenesTrabajo
            .Include(o => o.Operaciones)
                .ThenInclude(op => op.SolicitudesMateriales)
            .Include(o => o.Solicitante)
            .Include(o => o.TecnicoAsignado)
            .Include(o => o.Dependencia)
            .Include(o => o.Encuesta)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
