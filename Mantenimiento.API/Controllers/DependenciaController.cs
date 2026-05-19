using ClosedXML.Excel;
using FluentValidation;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/dependencies")]
[Authorize(Roles = "Administrador")]
public class DependenciaController(
    IDependenciaService dependenciaService,
    IValidator<DependenciaDto> validator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DependenciaDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var data = await dependenciaService.GetAllAsync();
        return Ok(ApiResponse<PagedResult<DependenciaDto>>.Ok(
            PagedResult<DependenciaDto>.Create(data, page, pageSize),
            "Dependencias obtenidas correctamente."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DependenciaDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await dependenciaService.GetByIdAsync(id);
        return result != null
            ? Ok(ApiResponse<DependenciaDto>.Ok(result, "Dependencia encontrada."))
            : NotFound(ApiResponse<object>.Fail("Dependencia no encontrada."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DependenciaDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Create([FromBody] DependenciaDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Datos inválidos.", validation.Errors.Select(e => e.ErrorMessage)));

        var id = await dependenciaService.CreateAsync(dto);
        dto.Id = id;
        return CreatedAtAction(nameof(GetById), new { id },
            ApiResponse<DependenciaDto>.Ok(dto, "Dependencia creada correctamente."));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Update([FromBody] DependenciaDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Datos inválidos.", validation.Errors.Select(e => e.ErrorMessage)));

        await dependenciaService.UpdateAsync(dto);
        return Ok(ApiResponse.Ok("Dependencia actualizada correctamente."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await dependenciaService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Dependencia eliminada correctamente."));
    }

    // ── Carga masiva ──────────────────────────────────────────────────────────

    [HttpGet("template")]
    [ProducesResponseType(typeof(FileResult), 200)]
    public IActionResult DescargarPlantilla()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Dependencias");

        var headers = new[] { "Nombre *", "Codigo *", "Regional", "Departamento", "Ubicacion", "NombreContacto", "JefeEmail" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1ABC9C");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        ws.Cell(2, 1).Value = "Dependencia Norte";
        ws.Cell(2, 2).Value = "DEP01";
        ws.Cell(2, 3).Value = "Norte";
        ws.Cell(2, 4).Value = "Operaciones";
        ws.Cell(2, 5).Value = "Edificio A, Piso 2";
        ws.Cell(2, 6).Value = "Carlos López";
        ws.Cell(2, 7).Value = "carlos.lopez@empresa.com";
        ws.Row(2).Style.Font.Italic = true;
        ws.Row(2).Style.Font.FontColor = XLColor.Gray;

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "plantilla_dependencias.xlsx");
    }

    [HttpPost("preview-excel")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<List<DependenciaPreviewRow>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> PreviewExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Debe adjuntar un archivo Excel."));

        var existentes = await dependenciaService.GetAllAsync();
        var codigosExistentes = existentes
            .Select(d => d.Codigo.ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rows = new List<DependenciaPreviewRow>();
        var codigosEnArchivo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var stream = file.OpenReadStream();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.First();

        int fila = 2;
        while (true)
        {
            var row = ws.Row(fila);
            if (row.Cell(1).IsEmpty() && row.Cell(2).IsEmpty()) break;

            var preview = new DependenciaPreviewRow { Fila = fila };
            var errores = new List<string>();

            preview.Nombre         = row.Cell(1).GetString().Trim();
            preview.Codigo         = row.Cell(2).GetString().Trim();
            preview.Regional       = row.Cell(3).GetString().Trim();
            preview.Departamento   = row.Cell(4).GetString().Trim();
            preview.Ubicacion      = row.Cell(5).GetString().Trim();
            preview.NombreContacto = row.Cell(6).GetString().Trim();
            preview.JefeEmail      = row.Cell(7).GetString().Trim();

            if (string.IsNullOrWhiteSpace(preview.Nombre))
                errores.Add("Nombre es requerido");

            if (string.IsNullOrWhiteSpace(preview.Codigo))
                errores.Add("Codigo es requerido");
            else if (codigosExistentes.Contains(preview.Codigo))
                errores.Add($"El código '{preview.Codigo}' ya existe en el sistema");
            else if (!codigosEnArchivo.Add(preview.Codigo))
                errores.Add($"El código '{preview.Codigo}' está duplicado en el archivo");

            if (!string.IsNullOrWhiteSpace(preview.JefeEmail) && !IsValidEmail(preview.JefeEmail))
                errores.Add("JefeEmail no tiene formato válido");

            preview.EsValido = errores.Count == 0;
            preview.Error    = errores.Count > 0 ? string.Join("; ", errores) : null;
            rows.Add(preview);
            fila++;
        }

        return Ok(ApiResponse<List<DependenciaPreviewRow>>.Ok(rows,
            $"{rows.Count} fila(s): {rows.Count(r => r.EsValido)} válidas, {rows.Count(r => !r.EsValido)} con error."));
    }

    [HttpPost("bulk")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> BulkCreate([FromBody] List<DependenciaDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<object>.Fail("No se enviaron dependencias."));

        int creadas = 0;
        foreach (var dto in items)
        {
            await dependenciaService.CreateAsync(dto);
            creadas++;
        }

        return Ok(ApiResponse<object>.Ok(new { creadas }, $"{creadas} dependencia(s) cargada(s) correctamente."));
    }

    private static bool IsValidEmail(string email)
    {
        try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email; }
        catch { return false; }
    }
}
