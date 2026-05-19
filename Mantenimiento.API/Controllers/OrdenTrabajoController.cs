using FluentValidation;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Mantenimiento.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrdenTrabajoController(
    IOrdenTrabajoService ordenTrabajoService,
    IValidator<CrearOrdenRequest> validator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Administrador,Tecnico")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrdenTrabajoDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetAll([FromQuery] OrdenTrabajoFilter filter)
    {
        var result = await ordenTrabajoService.GetFilteredAsync(filter);
        return result.IsSuccess
            ? Ok(ApiResponse<PagedResult<OrdenTrabajoDto>>.Ok(
                PagedResult<OrdenTrabajoDto>.Create(result.Value!, filter.Page, filter.PageSize),
                "Órdenes obtenidas correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al obtener las órdenes."));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Solicitante")]
    [ProducesResponseType(typeof(ApiResponse<OrdenTrabajoDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Datos inválidos.", validationResult.Errors.Select(e => e.ErrorMessage)));

        var result = await ordenTrabajoService.CrearOrdenAsync(request);
        return result.IsSuccess
            ? Ok(ApiResponse<OrdenTrabajoDto>.Ok(result.Value!, "Orden de trabajo creada correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al crear la orden."));
    }

    [HttpGet("technician/{id:guid}")]
    [Authorize(Roles = "Administrador,Tecnico")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrdenTrabajoDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetOrdenesPorTecnico(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await ordenTrabajoService.GetOrdenesByTecnicoIdAsync(id);
        return result.IsSuccess
            ? Ok(ApiResponse<PagedResult<OrdenTrabajoDto>>.Ok(
                PagedResult<OrdenTrabajoDto>.Create(result.Value!, page, pageSize),
                "Órdenes del técnico obtenidas."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al obtener las órdenes."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador,Tecnico,Solicitante,Programador")]
    [ProducesResponseType(typeof(ApiResponse<OrdenTrabajoDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await ordenTrabajoService.GetByIdAsync(id);
        return result.IsSuccess
            ? Ok(ApiResponse<OrdenTrabajoDto>.Ok(result.Value!, "Orden encontrada."))
            : NotFound(ApiResponse<object>.Fail(result.ErrorMessage ?? "Orden no encontrada."));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Administrador,Tecnico")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> ActualizarEstado(Guid id, [FromBody] ActualizarEstadoOrdenRequest request)
    {
        var result = await ordenTrabajoService.ActualizarEstadoOrdenAsync(id, request.NuevoEstado);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Estado de la orden actualizado correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al actualizar el estado."));
    }

    [HttpPost("{id:guid}/operations")]
    [Authorize(Roles = "Administrador,Tecnico,Programador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> AgregarOperacion(Guid id, [FromBody] AgregarOperacionRequest request)
    {
        var result = await ordenTrabajoService.AgregarOperacionAsync(id, request.Numero, request.Descripcion, request.HorasHombre);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok($"Operación {request.Numero} agregada correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al agregar la operación."));
    }

    [HttpPut("operations/{operacionId:guid}/status")]
    [Authorize(Roles = "Tecnico")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> ActualizarEstadoOperacion(Guid operacionId, [FromBody] ActualizarEstadoOperacionRequest request)
    {
        var tecnicoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(tecnicoIdClaim, out var tecnicoId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al técnico."));

        var result = await ordenTrabajoService.ActualizarEstadoOperacionAsync(operacionId, request.NuevoEstado, tecnicoId);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok($"Estado de operación actualizado a {request.NuevoEstado}."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al actualizar el estado."));
    }

    [HttpDelete("operations/{operacionId:guid}")]
    [Authorize(Roles = "Administrador,Tecnico,Programador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> EliminarOperacion(Guid operacionId)
    {
        var result = await ordenTrabajoService.EliminarOperacionAsync(operacionId);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Operación eliminada correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al eliminar la operación."));
    }

    [HttpPut("operations/{operacionId:guid}")]
    [Authorize(Roles = "Administrador,Tecnico,Programador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> EditarOperacion(Guid operacionId, [FromBody] EditarOperacionRequest request)
    {
        var result = await ordenTrabajoService.EditarOperacionAsync(operacionId, request.Numero, request.Descripcion, request.HorasHombre);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Operación actualizada correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al editar la operación."));
    }

    [HttpPost("operations/{operacionId:guid}/activity")]
    [Authorize(Roles = "Tecnico")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> RegistrarActividad(Guid operacionId, [FromBody] RegistrarActividadRequest request)
    {
        var tecnicoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(tecnicoIdClaim, out var tecnicoId))
            return Unauthorized(ApiResponse<object>.Fail("No se pudo identificar al técnico."));

        var result = await ordenTrabajoService.RegistrarActividadAsync(operacionId, request, tecnicoId);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Actividad registrada correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al registrar actividad."));
    }
}

public class AgregarOperacionRequest   { public int Numero { get; set; } public string Descripcion { get; set; } = string.Empty; public int HorasHombre { get; set; } }
public class ActualizarEstadoOrdenRequest { public EstadoOrden NuevoEstado { get; set; } }
public class ActualizarEstadoOperacionRequest { public EstadoOperacion NuevoEstado { get; set; } }
public class EditarOperacionRequest    { public int Numero { get; set; } public string Descripcion { get; set; } = string.Empty; public int HorasHombre { get; set; } }
