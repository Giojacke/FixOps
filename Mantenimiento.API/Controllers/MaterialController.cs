using ClosedXML.Excel;
using FluentValidation;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/materials")]
[Authorize]
public class MaterialController(
    IMaterialService materialService,
    IValidator<MaterialDto> validator) : ControllerBase
{
    // ── Lectura ──────────────────────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MaterialDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 200)
    {
        var data = await materialService.GetAllAsync();
        return Ok(ApiResponse<PagedResult<MaterialDto>>.Ok(
            PagedResult<MaterialDto>.Create(data, page, pageSize),
            "Materiales obtenidos correctamente."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MaterialDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await materialService.GetByIdAsync(id);
        return result != null
            ? Ok(ApiResponse<MaterialDto>.Ok(result, "Material encontrado."))
            : NotFound(ApiResponse<object>.Fail("Material no encontrado."));
    }

    // ── CRUD ─────────────────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Administrador,Programador")]
    [ProducesResponseType(typeof(ApiResponse<MaterialDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Create([FromBody] MaterialDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Datos inválidos.", validation.Errors.Select(e => e.ErrorMessage)));

        var id = await materialService.CreateAsync(dto);
        dto.Id = id;
        return CreatedAtAction(nameof(GetById), new { id },
            ApiResponse<MaterialDto>.Ok(dto, "Material creado correctamente."));
    }

    [HttpPut]
    [Authorize(Roles = "Administrador,Programador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Update([FromBody] MaterialDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Datos inválidos.", validation.Errors.Select(e => e.ErrorMessage)));

        await materialService.UpdateAsync(dto);
        return Ok(ApiResponse.Ok("Material actualizado correctamente."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Programador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await materialService.DeleteAsync(id);
        return deleted
            ? Ok(ApiResponse.Ok("Material eliminado correctamente."))
            : BadRequest(ApiResponse<object>.Fail("El material está en uso y no puede eliminarse."));
    }

    // ── Ajuste de stock ───────────────────────────────────────────────────────

    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Administrador,Programador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> AjustarStock(Guid id, [FromBody] AjustarStockRequest request)
    {
        var result = await materialService.AjustarStockAsync(id, request.Ajuste);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok($"Stock ajustado en {(request.Ajuste >= 0 ? "+" : "")}{request.Ajuste} unidades."))
            : BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Error al ajustar stock."));
    }

    // ── Exportación Excel ─────────────────────────────────────────────────────

    [HttpGet("export")]
    [Authorize(Roles = "Administrador,Programador")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> ExportExcel()
    {
        var materiales = (await materialService.GetAllAsync()).ToList();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Materiales");

        var headers = new[] { "Nombre", "Tipo Material", "Descripción", "Stock Actual", "Precio Unitario" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var m in materiales)
        {
            ws.Cell(row, 1).Value = m.Nombre;
            ws.Cell(row, 2).Value = m.TipoMaterial;
            ws.Cell(row, 3).Value = m.Descripcion;
            ws.Cell(row, 4).Value = m.StockActual;
            ws.Cell(row, 5).Value = (double)m.PrecioUnitario;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"materiales_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("template")]
    [Authorize(Roles = "Administrador,Programador")]
    [ProducesResponseType(typeof(FileResult), 200)]
    public IActionResult DescargarPlantilla()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Materiales");

        var headers = new[] { "Nombre *", "TipoMaterial *", "Descripcion", "StockActual *", "PrecioUnitario *" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1ABC9C");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        ws.Cell(2, 1).Value = "Aceite Hidráulico";
        ws.Cell(2, 2).Value = "Lubricante";
        ws.Cell(2, 3).Value = "Aceite para sistemas hidráulicos";
        ws.Cell(2, 4).Value = 50;
        ws.Cell(2, 5).Value = 12.50;
        ws.Row(2).Style.Font.Italic = true;
        ws.Row(2).Style.Font.FontColor = XLColor.Gray;

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "plantilla_materiales.xlsx");
    }

    // ── Carga masiva ──────────────────────────────────────────────────────────

    [HttpPost("preview-excel")]
    [Authorize(Roles = "Administrador,Programador")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<List<MaterialPreviewRow>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public IActionResult PreviewExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Debe adjuntar un archivo Excel."));

        var rows = new List<MaterialPreviewRow>();
        using var stream = file.OpenReadStream();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.First();

        int fila = 2;
        while (true)
        {
            var row = ws.Row(fila);
            if (row.Cell(1).IsEmpty() && row.Cell(2).IsEmpty()) break;

            var preview = new MaterialPreviewRow { Fila = fila };
            var errores = new List<string>();

            preview.Nombre       = row.Cell(1).GetString().Trim();
            preview.TipoMaterial = row.Cell(2).GetString().Trim();
            preview.Descripcion  = row.Cell(3).GetString().Trim();

            if (!int.TryParse(row.Cell(4).GetString(), out var stock) || stock < 0)
                errores.Add("StockActual debe ser un número entero ≥ 0");
            else
                preview.StockActual = stock;

            if (!decimal.TryParse(row.Cell(5).GetString().Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var precio) || precio < 0)
                errores.Add("PrecioUnitario debe ser un número ≥ 0");
            else
                preview.PrecioUnitario = precio;

            if (string.IsNullOrWhiteSpace(preview.Nombre))       errores.Add("Nombre es requerido");
            if (string.IsNullOrWhiteSpace(preview.TipoMaterial)) errores.Add("TipoMaterial es requerido");

            preview.EsValido = errores.Count == 0;
            preview.Error    = errores.Count > 0 ? string.Join("; ", errores) : null;
            rows.Add(preview);
            fila++;
        }

        return Ok(ApiResponse<List<MaterialPreviewRow>>.Ok(rows,
            $"{rows.Count} fila(s): {rows.Count(r => r.EsValido)} válidas, {rows.Count(r => !r.EsValido)} con error."));
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Administrador,Programador")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> BulkCreate([FromBody] List<MaterialDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<object>.Fail("No se enviaron materiales."));

        int creados = 0;
        foreach (var dto in items) { await materialService.CreateAsync(dto); creados++; }

        return Ok(ApiResponse<object>.Ok(new { creados }, $"{creados} material(es) cargado(s) correctamente."));
    }
}
