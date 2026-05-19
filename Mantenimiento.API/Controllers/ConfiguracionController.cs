using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/configuracion")]
[Authorize(Roles = "Administrador")]
public class ConfiguracionController(IConfiguracionCorreoService service) : ControllerBase
{
    [HttpGet("correo")]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracionCorreoDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Get()
    {
        var dto = await service.GetAsync();
        return Ok(ApiResponse<ConfiguracionCorreoDto>.Ok(dto, "Configuración obtenida."));
    }

    [HttpPut("correo")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Update([FromBody] ConfiguracionCorreoDto dto)
    {
        var result = await service.UpdateAsync(dto);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Configuración guardada correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al guardar."));
    }

    [HttpPost("correo/prueba")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> EnviarPrueba([FromBody] PruebaCorreoRequest request)
    {
        var result = await service.EnviarPruebaAsync(request.EmailDestino);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Correo de prueba enviado correctamente."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al enviar."));
    }
}
