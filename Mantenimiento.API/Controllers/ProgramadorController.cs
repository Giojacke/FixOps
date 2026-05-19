using ClosedXML.Excel;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/programador")]
[Authorize]
public class ProgramadorController(IProgramadorService svc) : ControllerBase
{
    private bool TryGetCurrentUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    // ── Técnicos a cargo ─────────────────────────────────────────────────────
    [HttpGet("tecnicos")]
    [Authorize(Roles = "Programador")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UsuarioDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetTecnicos()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al usuario autenticado."));

        var r = await svc.GetTecnicosACargo(currentUserId);
        return r.IsSuccess
            ? Ok(ApiResponse<IEnumerable<UsuarioDto>>.Ok(r.Value!, "Técnicos obtenidos."))
            : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    // ── Órdenes de los técnicos ───────────────────────────────────────────────
    [HttpGet("ordenes")]
    [Authorize(Roles = "Programador")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrdenTrabajoDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetOrdenes()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al usuario autenticado."));

        var r = await svc.GetOrdenesDeTenicosAsync(currentUserId);
        return r.IsSuccess
            ? Ok(ApiResponse<IEnumerable<OrdenTrabajoDto>>.Ok(r.Value!, "Órdenes obtenidas."))
            : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    // ── Exportación Excel de órdenes ─────────────────────────────────────────
    [HttpGet("ordenes/export")]
    [Authorize(Roles = "Programador")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> ExportOrdenesExcel()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al usuario autenticado."));

        var r = await svc.GetOrdenesDeTenicosAsync(currentUserId);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));

        var ordenes = r.Value!.ToList();

        using var wb = new XLWorkbook();

        // ── Hoja 1: Órdenes ──────────────────────────────────────────────────
        var wsO = wb.Worksheets.Add("Órdenes");
        var headersO = new[] { "Folio", "Descripción", "Estado", "Urgencia", "Técnico Asignado", "Dependencia", "Operaciones" };
        for (int i = 0; i < headersO.Length; i++)
        {
            wsO.Cell(1, i + 1).Value = headersO[i];
            wsO.Cell(1, i + 1).Style.Font.Bold = true;
            wsO.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
            wsO.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }
        int rowO = 2;
        foreach (var o in ordenes)
        {
            wsO.Cell(rowO, 1).Value = o.Folio;
            wsO.Cell(rowO, 2).Value = o.Descripcion;
            wsO.Cell(rowO, 3).Value = o.Estado.ToString();
            wsO.Cell(rowO, 4).Value = o.Urgencia.ToString();
            wsO.Cell(rowO, 5).Value = o.TecnicoAsignadoNombre ?? "—";
            wsO.Cell(rowO, 6).Value = o.DependenciaNombre ?? "—";
            wsO.Cell(rowO, 7).Value = o.Operaciones?.Count() ?? 0;
            rowO++;
        }
        wsO.Columns().AdjustToContents();

        // ── Hoja 2: Operaciones ──────────────────────────────────────────────
        var wsP = wb.Worksheets.Add("Operaciones");
        var headersP = new[] { "Folio Orden", "N° Op.", "Función", "Horas H.", "Estado", "Técnico", "Fecha Inicio", "Fecha Fin" };
        for (int i = 0; i < headersP.Length; i++)
        {
            wsP.Cell(1, i + 1).Value = headersP[i];
            wsP.Cell(1, i + 1).Style.Font.Bold = true;
            wsP.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1ABC9C");
            wsP.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }
        int rowP = 2;
        foreach (var o in ordenes)
        {
            foreach (var op in (o.Operaciones ?? Enumerable.Empty<OperacionDto>()).OrderBy(x => x.Numero))
            {
                wsP.Cell(rowP, 1).Value = o.Folio;
                wsP.Cell(rowP, 2).Value = op.Numero;
                wsP.Cell(rowP, 3).Value = op.Descripcion;
                wsP.Cell(rowP, 4).Value = op.HorasHombre;
                wsP.Cell(rowP, 5).Value = op.Estado.ToString();
                wsP.Cell(rowP, 6).Value = o.TecnicoAsignadoNombre ?? "—";
                wsP.Cell(rowP, 7).Value = op.FechaInicio?.ToString("dd/MM/yyyy HH:mm") ?? "—";
                wsP.Cell(rowP, 8).Value = op.FechaFin?.ToString("dd/MM/yyyy HH:mm") ?? "—";
                rowP++;
            }
        }
        wsP.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ordenes_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // ── Notificaciones ────────────────────────────────────────────────────────
    [HttpGet("notificaciones")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificacionDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> GetNotificaciones([FromQuery] bool soloNoLeidas = false)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al usuario autenticado."));

        var r = await svc.GetNotificacionesAsync(currentUserId, soloNoLeidas);
        return r.IsSuccess
            ? Ok(ApiResponse<IEnumerable<NotificacionDto>>.Ok(r.Value!, "Notificaciones obtenidas."))
            : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    [HttpPut("notificaciones/{id:guid}/leida")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> MarcarLeida(Guid id)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al usuario autenticado."));

        var r = await svc.MarcarLeidaAsync(id, currentUserId);
        return r.IsSuccess ? Ok(ApiResponse.Ok("Marcada como leída.")) : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    [HttpPut("notificaciones/leer-todas")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al usuario autenticado."));

        await svc.MarcarTodasLeidasAsync(currentUserId);
        return Ok(ApiResponse.Ok("Todas marcadas como leídas."));
    }

    // ── Aprobación de materiales ──────────────────────────────────────────────
    [HttpPost("aprobar-materiales")]
    [Authorize(Roles = "Programador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> AprobarMateriales([FromBody] AprobacionMaterialRequest request)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al usuario autenticado."));

        var r = await svc.AprobarMaterialesYNotificarAsync(request, currentUserId);
        return r.IsSuccess ? Ok(ApiResponse.Ok("Materiales aprobados y técnico notificado.")) : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    // ── Horarios ──────────────────────────────────────────────────────────────
    [HttpGet("turnos/{tecnicoId:guid}")]
    [Authorize(Roles = "Programador,Tecnico,Administrador")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TurnoHorarioDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetTurnos(Guid tecnicoId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta)
    {
        // Un técnico solo puede consultar sus propios turnos
        if (User.IsInRole("Tecnico"))
        {
            if (!TryGetCurrentUserId(out var myId) || myId != tecnicoId)
                return Forbid();
        }

        var r = await svc.GetTurnosAsync(tecnicoId, desde, hasta);
        return r.IsSuccess
            ? Ok(ApiResponse<IEnumerable<TurnoHorarioDto>>.Ok(r.Value!, "Turnos obtenidos."))
            : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    [HttpPost("turnos/sugerir")]
    [Authorize(Roles = "Programador")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TurnoHorarioDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> SugerirHorario([FromBody] SugerirHorarioRequest request)
    {
        var r = await svc.SugerirHorarioAsync(request);
        return r.IsSuccess
            ? Ok(ApiResponse<IEnumerable<TurnoHorarioDto>>.Ok(r.Value!, "Horario sugerido."))
            : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    [HttpPost("turnos")]
    [Authorize(Roles = "Programador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GuardarTurnos([FromBody] GuardarTurnosRequest request)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al usuario autenticado."));

        var r = await svc.GuardarTurnosAsync(request, currentUserId);
        return r.IsSuccess ? Ok(ApiResponse.Ok("Turnos guardados.")) : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    [HttpDelete("turnos/{turnoId:guid}")]
    [Authorize(Roles = "Programador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> EliminarTurno(Guid turnoId)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al usuario autenticado."));

        var r = await svc.EliminarTurnoAsync(turnoId, currentUserId);
        return r.IsSuccess ? Ok(ApiResponse.Ok("Turno eliminado.")) : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    // ── Configuración empresa ─────────────────────────────────────────────────
    [HttpGet("configuracion")]
    [Authorize(Roles = "Administrador,Programador")]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracionEmpresaDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetConfiguracion()
    {
        var r = await svc.GetConfiguracionAsync();
        return r.IsSuccess
            ? Ok(ApiResponse<ConfiguracionEmpresaDto>.Ok(r.Value!, "Configuración obtenida."))
            : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    [HttpPut("configuracion")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> ActualizarConfiguracion([FromBody] ConfiguracionEmpresaDto dto)
    {
        var r = await svc.ActualizarConfiguracionAsync(dto);
        return r.IsSuccess ? Ok(ApiResponse.Ok("Configuración actualizada.")) : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    // ── Recomendaciones de horario ────────────────────────────────────────────
    [HttpGet("recomendaciones")]
    [Authorize(Roles = "Administrador,Programador")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RecomendacionHorarioDto>>), 200)]
    public async Task<IActionResult> GetRecomendaciones()
    {
        var r = await svc.GetRecomendacionesAsync();
        return Ok(ApiResponse<IEnumerable<RecomendacionHorarioDto>>.Ok(r.Value!, "Recomendaciones obtenidas."));
    }

    [HttpPost("recomendaciones")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<RecomendacionHorarioDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CrearRecomendacion([FromBody] RecomendacionHorarioDto dto)
    {
        var r = await svc.CrearRecomendacionAsync(dto);
        return r.IsSuccess
            ? Ok(ApiResponse<RecomendacionHorarioDto>.Ok(r.Value!, "Recomendación creada."))
            : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    [HttpPut("recomendaciones/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ActualizarRecomendacion(Guid id, [FromBody] RecomendacionHorarioDto dto)
    {
        dto.Id = id;
        var r = await svc.ActualizarRecomendacionAsync(dto);
        return r.IsSuccess ? Ok(ApiResponse.Ok("Recomendación actualizada.")) : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }

    [HttpDelete("recomendaciones/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> EliminarRecomendacion(Guid id)
    {
        var r = await svc.EliminarRecomendacionAsync(id);
        return r.IsSuccess ? Ok(ApiResponse.Ok("Recomendación eliminada.")) : BadRequest(ApiResponse<object>.Fail(r.ErrorMessage!));
    }
}
