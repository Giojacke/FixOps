using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/auditoria")]
[Authorize(Roles = "Administrador")]
public class AuditoriaController(IUnitOfWork unitOfWork) : ControllerBase
{
    /// <summary>
    /// Obtiene el registro de auditoría de eliminaciones y cancelaciones.
    /// Filtros opcionales: fechaDesde, fechaHasta, tipoEntidad (Orden|Operacion), usuarioEmail.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RegistroAuditoriaDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetAll([FromQuery] AuditoriaFilter filter)
    {
        var todos = await unitOfWork.Auditorias.GetAllAsync();

        var query = todos.AsQueryable();

        if (filter.FechaDesde.HasValue)
            query = query.Where(r => r.FechaHora >= filter.FechaDesde.Value);

        if (filter.FechaHasta.HasValue)
            query = query.Where(r => r.FechaHora <= filter.FechaHasta.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(filter.TipoEntidad))
            query = query.Where(r => r.TipoEntidad.Equals(filter.TipoEntidad, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(filter.UsuarioEmail))
            query = query.Where(r => r.UsuarioEmail.Contains(filter.UsuarioEmail, StringComparison.OrdinalIgnoreCase));

        var ordenado = query.OrderByDescending(r => r.FechaHora);
        var dtos     = ordenado.Select(ToDto);

        return Ok(ApiResponse<PagedResult<RegistroAuditoriaDto>>.Ok(
            PagedResult<RegistroAuditoriaDto>.Create(dtos, filter.Page, filter.PageSize),
            "Registros de auditoría obtenidos correctamente."));
    }

    /// <summary>
    /// Obtiene un registro de auditoría por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RegistroAuditoriaDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await unitOfWork.Auditorias.GetByIdAsync(id);
        return entity != null
            ? Ok(ApiResponse<RegistroAuditoriaDto>.Ok(ToDto(entity), "Registro encontrado."))
            : NotFound(ApiResponse<object>.Fail("Registro de auditoría no encontrado."));
    }

    private static RegistroAuditoriaDto ToDto(RegistroAuditoria r) => new()
    {
        Id             = r.Id,
        TipoEntidad    = r.TipoEntidad,
        EntidadId      = r.EntidadId,
        ResumenEntidad = r.ResumenEntidad,
        Accion         = r.Accion,
        FechaHora      = r.FechaHora,
        UsuarioId      = r.UsuarioId,
        UsuarioNombre  = r.UsuarioNombre,
        UsuarioEmail   = r.UsuarioEmail
    };
}
