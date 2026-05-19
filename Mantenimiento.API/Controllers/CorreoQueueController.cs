using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/correo-queue")]
[Authorize(Roles = "Administrador")]
public class CorreoQueueController(ICorreoQueueService queueService) : ControllerBase
{
    /// <summary>GET /api/v1/correo-queue — lista paginada con filtros opcionales.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CorreoEncoladoDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetAll([FromQuery] CorreoQueueFilter filter)
    {
        var items = await queueService.GetAllAsync(filter);
        return Ok(ApiResponse<IEnumerable<CorreoEncoladoDto>>.Ok(items, "Cola de correos obtenida."));
    }

    /// <summary>GET /api/v1/correo-queue/stats — conteos por estado.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<CorreoQueueStatsDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetStats()
    {
        var stats = await queueService.GetStatsAsync();
        return Ok(ApiResponse<CorreoQueueStatsDto>.Ok(stats, "Estadísticas calculadas."));
    }

    /// <summary>GET /api/v1/correo-queue/{id} — detalle de un correo.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CorreoEncoladoDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await queueService.GetByIdAsync(id);
        return dto == null
            ? NotFound(ApiResponse<object>.Fail("Correo no encontrado."))
            : Ok(ApiResponse<CorreoEncoladoDto>.Ok(dto, "Correo encontrado."));
    }

    /// <summary>POST /api/v1/correo-queue/{id}/reintentar — reencola un correo específico.</summary>
    [HttpPost("{id:guid}/reintentar")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Reintentar(Guid id)
    {
        var result = await queueService.ReintentarAsync(id);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Correo reencolado correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al reencolar el correo."));
    }

    /// <summary>POST /api/v1/correo-queue/reintentar-fallidos — reencola todos los fallidos.</summary>
    [HttpPost("reintentar-fallidos")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> ReintentarFallidos()
    {
        var count = await queueService.ReintentarFallidosAsync();
        return Ok(ApiResponse<object>.Ok(new { reencolados = count },
            $"{count} correo(s) fallido(s) reencolados."));
    }
}
