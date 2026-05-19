using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Mantenimiento.Application.Mappings;
using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Enums;
using Mantenimiento.Domain.Interfaces;

namespace Mantenimiento.Application.Services;

public class ProgramadorService(IUnitOfWork uow) : IProgramadorService
{
    // ── Técnicos a cargo ─────────────────────────────────────────────────────
    public async Task<Result<IEnumerable<UsuarioDto>>> GetTecnicosACargo(Guid programadorId)
    {
        var todos = await uow.Usuarios.GetAllAsync();
        var tecnicos = todos
            .Where(u => u.ProgramadorId == programadorId && u.Rol == RolUsuario.Tecnico)
            .Select(u => u.ToDto());
        return Result<IEnumerable<UsuarioDto>>.Success(tecnicos);
    }

    // ── Órdenes de sus técnicos ───────────────────────────────────────────────
    public async Task<Result<IEnumerable<OrdenTrabajoDto>>> GetOrdenesDeTenicosAsync(Guid programadorId)
    {
        var todos = await uow.Usuarios.GetAllAsync();
        var tecnicoIds = todos
            .Where(u => u.ProgramadorId == programadorId && u.Rol == RolUsuario.Tecnico)
            .Select(u => u.Id)
            .ToHashSet();

        var ordenes = await uow.OrdenesTrabajo.GetFilteredAsync();
        var filtradas = ordenes
            .Where(o => o.TecnicoAsignadoId.HasValue && tecnicoIds.Contains(o.TecnicoAsignadoId.Value))
            .Select(o => o.ToDto());

        return Result<IEnumerable<OrdenTrabajoDto>>.Success(filtradas);
    }

    // ── Notificaciones ────────────────────────────────────────────────────────
    public async Task<Result<IEnumerable<NotificacionDto>>> GetNotificacionesAsync(Guid usuarioId, bool soloNoLeidas = false)
    {
        var todas = await uow.Notificaciones.GetAllAsync();
        var q = todas.Where(n => n.DestinatarioId == usuarioId);
        if (soloNoLeidas) q = q.Where(n => !n.Leida);

        var dtos = q.OrderByDescending(n => n.FechaCreacion).Select(n => MapNotificacion(n));
        return Result<IEnumerable<NotificacionDto>>.Success(dtos);
    }

    public async Task<Result> MarcarLeidaAsync(Guid notificacionId, Guid usuarioId)
    {
        var n = await uow.Notificaciones.GetByIdAsync(notificacionId);
        if (n == null || n.DestinatarioId != usuarioId) return Result.Failure("Notificación no encontrada.");
        n.Leida = true;
        uow.Notificaciones.Update(n);
        await uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> MarcarTodasLeidasAsync(Guid usuarioId)
    {
        var todas = await uow.Notificaciones.GetAllAsync();
        foreach (var n in todas.Where(n => n.DestinatarioId == usuarioId && !n.Leida))
        {
            n.Leida = true;
            uow.Notificaciones.Update(n);
        }
        await uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> EnviarNotificacionAsync(Guid destinatarioId, string mensaje, Guid? ordenId, Guid? operacionId)
    {
        await uow.Notificaciones.AddAsync(new Notificacion
        {
            Id             = Guid.NewGuid(),
            DestinatarioId = destinatarioId,
            Mensaje        = mensaje,
            OrdenTrabajoId = ordenId,
            OperacionId    = operacionId,
            Leida          = false,
            FechaCreacion  = DateTime.UtcNow
        });
        await uow.SaveChangesAsync();
        return Result.Success();
    }

    // ── Aprobación de materiales ──────────────────────────────────────────────
    public async Task<Result> AprobarMaterialesYNotificarAsync(AprobacionMaterialRequest request, Guid programadorId)
    {
        // Verificar que el técnico está bajo el mando del programador
        var tecnico = await uow.Usuarios.GetByIdAsync(request.TecnicoId);
        if (tecnico == null || tecnico.ProgramadorId != programadorId)
            return Result.Failure("El técnico no está bajo su mando.");

        var operacion = await uow.Operaciones.GetByIdAsync(request.OperacionId);
        if (operacion == null) return Result.Failure("Operación no encontrada.");

        var orden = await uow.OrdenesTrabajo.GetByIdAsync(operacion.OrdenTrabajoId);
        if (orden == null) return Result.Failure("Orden no encontrada.");

        // Verificar y descontar stock de materiales de catálogo
        var solicitudes = (await uow.SolicitudesMateriales.GetAllAsync())
            .Where(s => s.OperacionId == request.OperacionId && !s.EsPersonalizado && s.MaterialId.HasValue)
            .ToList();

        foreach (var sol in solicitudes)
        {
            var material = await uow.Materiales.GetByIdAsync(sol.MaterialId!.Value);
            if (material == null) continue;
            if (material.StockActual < sol.Cantidad)
                return Result.Failure($"Stock insuficiente para '{material.Nombre}'. Disponible: {material.StockActual}, requerido: {sol.Cantidad}.");
        }

        // Descontar stock
        foreach (var sol in solicitudes)
        {
            var material = await uow.Materiales.GetByIdAsync(sol.MaterialId!.Value);
            if (material == null) continue;
            material.StockActual -= sol.Cantidad;
            uow.Materiales.Update(material);
        }

        // Enviar notificación al técnico
        var msg = $"✅ Materiales aprobados para la Orden {orden.Folio} – Operación {operacion.Numero}. " +
                  (string.IsNullOrWhiteSpace(request.MensajeAdicional) ? "Puede proceder." : request.MensajeAdicional);

        await uow.Notificaciones.AddAsync(new Notificacion
        {
            Id             = Guid.NewGuid(),
            DestinatarioId = request.TecnicoId,
            Mensaje        = msg,
            OrdenTrabajoId = orden.Id,
            OperacionId    = request.OperacionId,
            Leida          = false,
            FechaCreacion  = DateTime.UtcNow
        });

        await uow.SaveChangesAsync();
        return Result.Success();
    }

    // ── Horarios ──────────────────────────────────────────────────────────────
    public async Task<Result<IEnumerable<TurnoHorarioDto>>> GetTurnosAsync(Guid tecnicoId, DateOnly desde, DateOnly hasta)
    {
        var todos = await uow.Turnos.GetAllAsync();
        var dtos = todos
            .Where(t => t.TecnicoId == tecnicoId && t.Fecha >= desde && t.Fecha <= hasta)
            .OrderBy(t => t.Fecha)
            .Select(t => MapTurno(t));
        return Result<IEnumerable<TurnoHorarioDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<TurnoHorarioDto>>> SugerirHorarioAsync(SugerirHorarioRequest request)
    {
        var config = await GetConfigOrDefault();
        var turnos = new List<TurnoHorarioDto>();

        int diasPeriodo = request.Periodo switch
        {
            "Quincenal" => 14,
            "Mensual"   => DateTime.DaysInMonth(request.FechaInicio.Year, request.FechaInicio.Month),
            _           => 7
        };

        // Genera turnos L–S (excluye domingo = DayOfWeek.Sunday)
        for (int i = 0; i < diasPeriodo; i++)
        {
            var fecha = request.FechaInicio.AddDays(i);
            if (fecha.DayOfWeek == DayOfWeek.Sunday) continue;

            var horaInicio = config.HoraInicioDefault;
            // Si el almuerzo NO está pagado en la jornada, el turno es 8 h en sitio (7 efectivas + 1 almuerzo)
            int duracionTotal = config.AlmuerzoPagadoEnJornada
                ? config.HorasDiariasEfectivas
                : config.HorasDiariasEfectivas + 1;

            var horaFin = horaInicio.AddHours(duracionTotal);

            turnos.Add(new TurnoHorarioDto
            {
                Id              = Guid.NewGuid(),
                TecnicoId       = request.TecnicoId,
                Fecha           = fecha,
                HoraInicio      = horaInicio,
                HoraFin         = horaFin,
                IncluyeAlmuerzo = config.AlmuerzoPagadoEnJornada,
                HorasEfectivas  = config.HorasDiariasEfectivas
            });
        }

        return Result<IEnumerable<TurnoHorarioDto>>.Success(turnos);
    }

    public async Task<Result> GuardarTurnosAsync(GuardarTurnosRequest request, Guid programadorId)
    {
        // Verificar propiedad
        var tecnico = await uow.Usuarios.GetByIdAsync(request.TecnicoId);
        if (tecnico == null || tecnico.ProgramadorId != programadorId)
            return Result.Failure("El técnico no está bajo su mando.");

        // Validar max 42 h semanales por semana
        var config = await GetConfigOrDefault();
        var porSemana = request.Turnos
            .GroupBy(t => ISOWeek(t.Fecha));
        foreach (var semana in porSemana)
        {
            var totalHoras = semana.Sum(t => (t.HoraFin - t.HoraInicio).TotalHours);
            if (totalHoras > config.HorasSemanalesMaximas)
                return Result.Failure($"La semana del {semana.Min(t => t.Fecha)} supera las {config.HorasSemanalesMaximas} h semanales ({totalHoras:F1} h).");
        }

        // Eliminar turnos existentes en el rango y reemplazar
        if (request.Turnos.Any())
        {
            var minFecha = request.Turnos.Min(t => t.Fecha);
            var maxFecha = request.Turnos.Max(t => t.Fecha);
            var existentes = (await uow.Turnos.GetAllAsync())
                .Where(t => t.TecnicoId == request.TecnicoId && t.Fecha >= minFecha && t.Fecha <= maxFecha)
                .ToList();
            foreach (var t in existentes) uow.Turnos.Remove(t);
        }

        foreach (var dto in request.Turnos)
        {
            await uow.Turnos.AddAsync(new TurnoHorario
            {
                Id              = Guid.NewGuid(),
                TecnicoId       = request.TecnicoId,
                Fecha           = dto.Fecha,
                HoraInicio      = dto.HoraInicio,
                HoraFin         = dto.HoraFin,
                IncluyeAlmuerzo = dto.IncluyeAlmuerzo
            });
        }

        await uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> EliminarTurnoAsync(Guid turnoId, Guid programadorId)
    {
        var turno = await uow.Turnos.GetByIdAsync(turnoId);
        if (turno == null) return Result.Failure("Turno no encontrado.");

        var tecnico = await uow.Usuarios.GetByIdAsync(turno.TecnicoId);
        if (tecnico?.ProgramadorId != programadorId)
            return Result.Failure("No tiene permiso para eliminar este turno.");

        uow.Turnos.Remove(turno);
        await uow.SaveChangesAsync();
        return Result.Success();
    }

    // ── Configuración empresa ────────────────────────────────────────────────
    public async Task<Result<ConfiguracionEmpresaDto>> GetConfiguracionAsync()
    {
        var config = await GetConfigOrDefault();
        return Result<ConfiguracionEmpresaDto>.Success(new ConfiguracionEmpresaDto
        {
            Id                      = config.Id,
            AlmuerzoPagadoEnJornada = config.AlmuerzoPagadoEnJornada,
            HorasDiariasEfectivas   = config.HorasDiariasEfectivas,
            HorasSemanalesMaximas   = config.HorasSemanalesMaximas,
            HorasExtrasMaximas      = config.HorasExtrasMaximas,
            HoraInicioDefault       = config.HoraInicioDefault
        });
    }

    public async Task<Result> ActualizarConfiguracionAsync(ConfiguracionEmpresaDto dto)
    {
        var config = await GetConfigOrDefault();
        config.AlmuerzoPagadoEnJornada = dto.AlmuerzoPagadoEnJornada;
        config.HorasDiariasEfectivas   = dto.HorasDiariasEfectivas;
        config.HorasSemanalesMaximas   = dto.HorasSemanalesMaximas;
        config.HorasExtrasMaximas      = dto.HorasExtrasMaximas;
        config.HoraInicioDefault       = dto.HoraInicioDefault;
        uow.ConfiguracionEmpresa.Update(config);
        await uow.SaveChangesAsync();
        return Result.Success();
    }

    // ── Recomendaciones de horario ────────────────────────────────────────────
    public async Task<Result<IEnumerable<RecomendacionHorarioDto>>> GetRecomendacionesAsync()
    {
        var lista = await uow.Recomendaciones.GetAllAsync();
        var dtos  = lista.OrderBy(r => r.HoraInicio).Select(MapRecomendacion);
        return Result<IEnumerable<RecomendacionHorarioDto>>.Success(dtos);
    }

    public async Task<Result<RecomendacionHorarioDto>> CrearRecomendacionAsync(RecomendacionHorarioDto dto)
    {
        var entity = new Domain.Entities.RecomendacionHorario
        {
            Id         = Guid.NewGuid(),
            Nombre     = dto.Nombre.Trim(),
            HoraInicio = dto.HoraInicio,
            HoraFin    = dto.HoraFin,
            Activo     = dto.Activo
        };
        await uow.Recomendaciones.AddAsync(entity);
        await uow.SaveChangesAsync();
        return Result<RecomendacionHorarioDto>.Success(MapRecomendacion(entity));
    }

    public async Task<Result> ActualizarRecomendacionAsync(RecomendacionHorarioDto dto)
    {
        var entity = await uow.Recomendaciones.GetByIdAsync(dto.Id);
        if (entity == null) return Result.Failure("Recomendación no encontrada.");
        entity.Nombre     = dto.Nombre.Trim();
        entity.HoraInicio = dto.HoraInicio;
        entity.HoraFin    = dto.HoraFin;
        entity.Activo     = dto.Activo;
        uow.Recomendaciones.Update(entity);
        await uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> EliminarRecomendacionAsync(Guid id)
    {
        var entity = await uow.Recomendaciones.GetByIdAsync(id);
        if (entity == null) return Result.Failure("Recomendación no encontrada.");
        uow.Recomendaciones.Remove(entity);
        await uow.SaveChangesAsync();
        return Result.Success();
    }

    // ── Importación masiva de horarios ────────────────────────────────────────
    public async Task<ImportarHorariosResult> ImportarHorariosAsync(ImportarHorariosRequest request)
    {
        var resultado = new ImportarHorariosResult { TotalRecibidos = request.Items.Count };
        var config    = await GetConfigOrDefault();
        var maxHoras  = config.HorasSemanalesMaximas;

        // Agrupar items por email para procesar técnico por técnico
        var porEmail = request.Items.GroupBy(i => i.EmailTecnico.Trim().ToLowerInvariant());

        foreach (var grupo in porEmail)
        {
            var usuarios = await uow.Usuarios.FindAsync(u => u.Email.ToLower() == grupo.Key);
            var tecnico  = usuarios.FirstOrDefault();

            if (tecnico == null)
            {
                foreach (var _ in grupo)
                    resultado.Errores.Add($"Técnico no encontrado: {grupo.Key}");
                continue;
            }

            // Obtener turnos ya existentes del técnico para validar semanas
            var todasLasFechas = grupo.Select(i => i.Fecha).ToList();
            var desdeGlobal    = todasLasFechas.Min();
            var hastaGlobal    = todasLasFechas.Max();
            var existentes     = (await uow.Turnos.FindAsync(
                t => t.TecnicoId == tecnico.Id && t.Fecha >= desdeGlobal && t.Fecha <= hastaGlobal))
                .ToList();

            foreach (var item in grupo)
            {
                // Validar horas del turno
                var horasTurno = (item.HoraFin.ToTimeSpan() - item.HoraInicio.ToTimeSpan()).TotalHours;
                if (horasTurno <= 0)
                {
                    resultado.Errores.Add($"{grupo.Key} – {item.Fecha}: HoraFin debe ser posterior a HoraInicio.");
                    continue;
                }

                // Validar límite semanal
                var semana = ISOWeek(item.Fecha);
                var horasSemanales = existentes
                    .Where(t => ISOWeek(t.Fecha) == semana)
                    .Sum(t => (t.HoraFin.ToTimeSpan() - t.HoraInicio.ToTimeSpan()).TotalHours)
                    + horasTurno;

                if (horasSemanales > maxHoras)
                {
                    resultado.Errores.Add($"{grupo.Key} – {item.Fecha}: supera el máximo de {maxHoras} h semanales (semana {semana}).");
                    continue;
                }

                // Eliminar turno existente en esa fecha si hay uno
                var existente = existentes.FirstOrDefault(t => t.Fecha == item.Fecha);
                if (existente != null)
                    uow.Turnos.Remove(existente);

                var nuevo = new Domain.Entities.TurnoHorario
                {
                    Id             = Guid.NewGuid(),
                    TecnicoId      = tecnico.Id,
                    Fecha          = item.Fecha,
                    HoraInicio     = item.HoraInicio,
                    HoraFin        = item.HoraFin,
                    IncluyeAlmuerzo = item.IncluyeAlmuerzo
                };
                await uow.Turnos.AddAsync(nuevo);
                existentes.Add(nuevo); // para validaciones posteriores dentro del mismo grupo
                resultado.Importados++;
            }
        }

        if (resultado.Importados > 0)
            await uow.SaveChangesAsync();

        return resultado;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task<Domain.Entities.ConfiguracionEmpresa> GetConfigOrDefault()
    {
        var all = await uow.ConfiguracionEmpresa.GetAllAsync();
        return all.FirstOrDefault() ?? new Domain.Entities.ConfiguracionEmpresa();
    }

    private static NotificacionDto MapNotificacion(Notificacion n) => new()
    {
        Id              = n.Id,
        Mensaje         = n.Mensaje,
        OrdenTrabajoId  = n.OrdenTrabajoId,
        FolioOrden      = n.OrdenTrabajo?.Folio ?? string.Empty,
        OperacionId     = n.OperacionId,
        NumeroOperacion = n.Operacion?.Numero ?? 0,
        Leida           = n.Leida,
        FechaCreacion   = n.FechaCreacion
    };

    private static TurnoHorarioDto MapTurno(TurnoHorario t)
    {
        var horas = (t.HoraFin - t.HoraInicio).TotalHours;
        return new TurnoHorarioDto
        {
            Id              = t.Id,
            TecnicoId       = t.TecnicoId,
            TecnicoNombre   = t.Tecnico?.Nombre ?? string.Empty,
            Fecha           = t.Fecha,
            HoraInicio      = t.HoraInicio,
            HoraFin         = t.HoraFin,
            IncluyeAlmuerzo = t.IncluyeAlmuerzo,
            HorasEfectivas  = t.IncluyeAlmuerzo ? horas : horas - 1
        };
    }

    private static int ISOWeek(DateOnly fecha)
    {
        var d = fecha.ToDateTime(TimeOnly.MinValue);
        return System.Globalization.ISOWeek.GetWeekOfYear(d);
    }

    private static RecomendacionHorarioDto MapRecomendacion(Domain.Entities.RecomendacionHorario r) => new()
    {
        Id         = r.Id,
        Nombre     = r.Nombre,
        HoraInicio = r.HoraInicio,
        HoraFin    = r.HoraFin,
        Activo     = r.Activo
    };
}
