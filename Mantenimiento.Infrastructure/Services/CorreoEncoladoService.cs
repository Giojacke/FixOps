using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Mantenimiento.Infrastructure.Persistence;

namespace Mantenimiento.Infrastructure.Services;

public class CorreoEncoladoService(MantenimientoDbContext db) : ICorreoQueueService
{
    // ── Enqueue ───────────────────────────────────────────────────────────────
    public async Task<Guid> EnqueueAsync(string destinatario, string asunto, string cuerpo, string tipo = "General")
    {
        var correo = new CorreoEncolado
        {
            Id           = Guid.NewGuid(),
            Destinatario = destinatario,
            Asunto       = asunto,
            Cuerpo       = cuerpo,
            TipoCorreo   = tipo,
            Estado       = EstadosCorreo.Pendiente,
            FechaCreacion = DateTime.UtcNow,
            ProximoIntento = DateTime.UtcNow   // ready to send immediately
        };
        db.CorreosEncolados.Add(correo);
        await db.SaveChangesAsync();
        return correo.Id;
    }

    // ── Query ─────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<CorreoEncoladoDto>> GetAllAsync(CorreoQueueFilter filter)
    {
        var q = db.CorreosEncolados.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Estado))     q = q.Where(c => c.Estado     == filter.Estado);
        if (!string.IsNullOrWhiteSpace(filter.TipoCorreo)) q = q.Where(c => c.TipoCorreo == filter.TipoCorreo);
        if (filter.Desde.HasValue) q = q.Where(c => c.FechaCreacion >= filter.Desde.Value);
        if (filter.Hasta.HasValue) q = q.Where(c => c.FechaCreacion <= filter.Hasta.Value.Date.AddDays(1).AddTicks(-1));

        var items = await q
            .OrderByDescending(c => c.FechaCreacion)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return items.Select(ToDto);
    }

    public async Task<CorreoEncoladoDto?> GetByIdAsync(Guid id)
    {
        var c = await db.CorreosEncolados.FindAsync(id);
        return c == null ? null : ToDto(c);
    }

    public async Task<CorreoQueueStatsDto> GetStatsAsync()
    {
        var counts = await db.CorreosEncolados
            .GroupBy(c => c.Estado)
            .Select(g => new { Estado = g.Key, Count = g.Count() })
            .ToListAsync();

        return new CorreoQueueStatsDto
        {
            Pendientes = counts.FirstOrDefault(c => c.Estado == EstadosCorreo.Pendiente)?.Count  ?? 0,
            Procesando = counts.FirstOrDefault(c => c.Estado == EstadosCorreo.Procesando)?.Count ?? 0,
            Enviados   = counts.FirstOrDefault(c => c.Estado == EstadosCorreo.Enviado)?.Count    ?? 0,
            Fallidos   = counts.FirstOrDefault(c => c.Estado == EstadosCorreo.Fallido)?.Count    ?? 0,
            Total      = counts.Sum(c => c.Count)
        };
    }

    // ── Retry ─────────────────────────────────────────────────────────────────
    public async Task<Result> ReintentarAsync(Guid id)
    {
        var correo = await db.CorreosEncolados.FindAsync(id);
        if (correo == null) return Result.Failure("Correo no encontrado.");
        if (correo.Estado == EstadosCorreo.Enviado) return Result.Failure("El correo ya fue enviado.");

        correo.Estado         = EstadosCorreo.Pendiente;
        correo.Intentos       = 0;
        correo.ProximoIntento = DateTime.UtcNow;
        correo.ErrorMensaje   = null;

        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<int> ReintentarFallidosAsync()
    {
        var fallidos = await db.CorreosEncolados
            .Where(c => c.Estado == EstadosCorreo.Fallido)
            .ToListAsync();

        foreach (var c in fallidos)
        {
            c.Estado         = EstadosCorreo.Pendiente;
            c.Intentos       = 0;
            c.ProximoIntento = DateTime.UtcNow;
            c.ErrorMensaje   = null;
        }

        await db.SaveChangesAsync();
        return fallidos.Count;
    }

    // ── Worker helpers (internal, called by EmailQueueWorker) ─────────────────
    internal async Task<List<CorreoEncolado>> GetPendientesParaEnviarAsync()
        => await db.CorreosEncolados
            .Where(c => c.Estado == EstadosCorreo.Pendiente && c.ProximoIntento <= DateTime.UtcNow)
            .OrderBy(c => c.FechaCreacion)
            .Take(20) // max batch per cycle
            .ToListAsync();

    internal async Task MarcarProcesandoAsync(CorreoEncolado correo)
    {
        correo.Estado = EstadosCorreo.Procesando;
        await db.SaveChangesAsync();
    }

    internal async Task MarcarEnviadoAsync(CorreoEncolado correo)
    {
        correo.Estado             = EstadosCorreo.Enviado;
        correo.FechaEnvio         = DateTime.UtcNow;
        correo.FechaUltimoIntento = DateTime.UtcNow;
        correo.ErrorMensaje       = null;
        await db.SaveChangesAsync();
    }

    internal async Task MarcarFallidoOReintentarAsync(CorreoEncolado correo, string error)
    {
        correo.Intentos++;
        correo.FechaUltimoIntento = DateTime.UtcNow;
        correo.ErrorMensaje       = error;

        if (correo.Intentos >= correo.MaxIntentos)
        {
            correo.Estado = EstadosCorreo.Fallido;
        }
        else
        {
            // Exponential backoff: 5, 10, 20 minutes
            var delayMinutes = 5 * (int)Math.Pow(2, correo.Intentos - 1);
            correo.Estado         = EstadosCorreo.Pendiente;
            correo.ProximoIntento = DateTime.UtcNow.AddMinutes(delayMinutes);
        }

        await db.SaveChangesAsync();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static CorreoEncoladoDto ToDto(CorreoEncolado c) => new()
    {
        Id                 = c.Id,
        Destinatario       = c.Destinatario,
        Asunto             = c.Asunto,
        TipoCorreo         = c.TipoCorreo,
        Estado             = c.Estado,
        Intentos           = c.Intentos,
        MaxIntentos        = c.MaxIntentos,
        FechaCreacion      = c.FechaCreacion,
        FechaUltimoIntento = c.FechaUltimoIntento,
        FechaEnvio         = c.FechaEnvio,
        ProximoIntento     = c.ProximoIntento,
        ErrorMensaje       = c.ErrorMensaje
    };
}
