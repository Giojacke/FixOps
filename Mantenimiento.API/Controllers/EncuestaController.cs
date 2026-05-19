using FluentValidation;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/surveys")]
[Authorize]
public class EncuestaController(
    IEncuestaService encuestaService,
    IValidator<CrearEncuestaRequest> validator) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Registrar([FromBody] CrearEncuestaRequest request)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Datos inválidos.", validation.Errors.Select(e => e.ErrorMessage)));

        var result = await encuestaService.RegistrarEncuestaAsync(request);
        return result.IsSuccess
            ? Ok(ApiResponse<object>.Ok(new { id = result.Value }, "Encuesta registrada correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al registrar la encuesta."));
    }

    [HttpGet("results")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EncuestaDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetResultados(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var data = await encuestaService.GetResultadosAsync(desde, hasta);
        return Ok(ApiResponse<PagedResult<EncuestaDto>>.Ok(
            PagedResult<EncuestaDto>.Create(data, page, pageSize),
            "Resultados de encuestas obtenidos."));
    }

    [HttpGet("metrics")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ApiResponse<MetricasDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetMetricas()
    {
        var data = await encuestaService.GetMetricasAsync();
        return Ok(ApiResponse<MetricasDto>.Ok(data, "Métricas calculadas correctamente."));
    }
}
