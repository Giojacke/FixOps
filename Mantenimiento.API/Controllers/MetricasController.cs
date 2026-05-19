using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Domain.Enums;
using Mantenimiento.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/metricas")]
[Authorize(Roles = "Administrador")]
public class MetricasController(MantenimientoDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string?   regional = null,
        [FromQuery] DateTime? desde    = null,
        [FromQuery] DateTime? hasta    = null)
    {
        desde ??= DateTime.UtcNow.AddMonths(-6);
        hasta ??= DateTime.UtcNow;
        var hastaFin = hasta.Value.Date.AddDays(1).AddTicks(-1);

        // ── Regionales disponibles ────────────────────────────────
        var todasRegionales = await db.Dependencias
            .Where(d => d.Regional != null && d.Regional != string.Empty)
            .Select(d => d.Regional)
            .Distinct()
            .OrderBy(r => r)
            .ToListAsync();

        // ── Query base de órdenes ─────────────────────────────────
        var query = db.OrdenesTrabajo
            .Include(o => o.Dependencia)
            .Include(o => o.Operaciones)
            .Where(o => o.FechaCreacion >= desde && o.FechaCreacion <= hastaFin);

        if (!string.IsNullOrEmpty(regional))
            query = query.Where(o => o.Dependencia.Regional == regional);

        var ordenes = await query.ToListAsync();

        // ── KPIs ──────────────────────────────────────────────────
        var finalizadas = ordenes.Where(o => o.Estado == EstadoOrden.Finalizada).ToList();
        var tiemposHoras = finalizadas
            .Where(o => o.FechaFinalizacion.HasValue)
            .Select(o => (o.FechaFinalizacion!.Value - o.FechaCreacion).TotalHours)
            .ToList();

        var opsEnPausa = ordenes
            .SelectMany(o => o.Operaciones)
            .Count(op => op.Estado == EstadoOperacion.Pausa);

        var kpis = new DashboardKpis
        {
            TotalOrdenes        = ordenes.Count,
            TiempoPromedioHoras = tiemposHoras.Count > 0 ? Math.Round(tiemposHoras.Average(), 1) : 0,
            Pendientes          = ordenes.Count(o => o.Estado == EstadoOrden.Pendiente),
            EnProceso           = ordenes.Count(o => o.Estado == EstadoOrden.EnProceso),
            Finalizadas         = ordenes.Count(o => o.Estado == EstadoOrden.Finalizada),
            Canceladas          = ordenes.Count(o => o.Estado == EstadoOrden.Cancelada),
            OperacionesEnPausa  = opsEnPausa
        };

        // ── Órdenes por Estado ────────────────────────────────────
        var porEstado = new List<EstadoCountDto>
        {
            new() { Estado = "Pendiente", Cantidad = kpis.Pendientes },
            new() { Estado = "En Proceso", Cantidad = kpis.EnProceso },
            new() { Estado = "Finalizada", Cantidad = kpis.Finalizadas },
            new() { Estado = "Cancelada",  Cantidad = kpis.Canceladas }
        };

        // ── Métricas por Regional ─────────────────────────────────
        var porRegional = ordenes
            .GroupBy(o => o.Dependencia?.Regional ?? "Sin Regional")
            .Select(g =>
            {
                var tiempos = g
                    .Where(o => o.Estado == EstadoOrden.Finalizada && o.FechaFinalizacion.HasValue)
                    .Select(o => (o.FechaFinalizacion!.Value - o.FechaCreacion).TotalHours)
                    .ToList();
                return new RegionalMetricaDto
                {
                    Regional        = g.Key,
                    CantidadOrdenes = g.Count(),
                    PromedioHoras   = tiempos.Count > 0 ? Math.Round(tiempos.Average(), 1) : 0
                };
            })
            .OrderByDescending(r => r.CantidadOrdenes)
            .ToList();

        // ── Tendencia mensual ─────────────────────────────────────
        var tendencia = ordenes
            .GroupBy(o => new { o.FechaCreacion.Year, o.FechaCreacion.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new TendenciaMensualDto
            {
                Periodo     = $"{g.Key.Year}-{g.Key.Month:D2}",
                Creadas     = g.Count(),
                Finalizadas = g.Count(o => o.Estado == EstadoOrden.Finalizada)
            })
            .ToList();

        var resultado = new DashboardDto
        {
            Kpis                = kpis,
            OrdenesPorEstado    = porEstado,
            MetricasPorRegional = porRegional,
            Tendencia           = tendencia,
            Regionales          = todasRegionales
        };

        return Ok(ApiResponse<DashboardDto>.Ok(resultado, "Métricas obtenidas correctamente."));
    }
}
