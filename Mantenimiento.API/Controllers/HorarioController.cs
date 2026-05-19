using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/horarios")]
[Authorize(Roles = "Administrador,Programador")]
public class HorarioController(IProgramadorService programadorService) : ControllerBase
{
    /// <summary>
    /// Importa horarios de empleados en lote identificando al técnico por su email.
    /// Permite enviar desde sistemas externos sin conocer los GUIDs internos.
    /// </summary>
    /// <remarks>
    /// Ejemplo de body:
    /// <code>
    /// {
    ///   "items": [
    ///     {
    ///       "emailTecnico": "tecnico@empresa.com",
    ///       "fecha": "2026-05-12",
    ///       "horaInicio": "07:00:00",
    ///       "horaFin": "14:00:00",
    ///       "incluyeAlmuerzo": true
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </remarks>
    [HttpPost("importar")]
    [ProducesResponseType(typeof(ApiResponse<ImportarHorariosResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Importar([FromBody] ImportarHorariosRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            return BadRequest(ApiResponse<object>.Fail("La lista de horarios no puede estar vacía."));

        var resultado = await programadorService.ImportarHorariosAsync(request);

        return Ok(ApiResponse<ImportarHorariosResult>.Ok(
            resultado,
            $"Importación completada: {resultado.Importados} de {resultado.TotalRecibidos} registros procesados."));
    }
}
