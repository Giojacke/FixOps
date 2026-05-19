using ClosedXML.Excel;
using FluentValidation;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantenimiento.API.Controllers;

[ApiController]
[Route("api/v1/technicians")]
[Authorize(Roles = "Administrador")]
public class TecnicoController(
    ITecnicoService tecnicoService,
    IDependenciaService dependenciaService,
    IUserService userService,
    IValidator<UsuarioDto> validator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UsuarioDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 200)
    {
        var data = await tecnicoService.GetAllUsuariosAsync();
        return Ok(ApiResponse<PagedResult<UsuarioDto>>.Ok(
            PagedResult<UsuarioDto>.Create(data, page, pageSize),
            "Usuarios obtenidos correctamente."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await tecnicoService.GetByIdAsync(id);
        return result != null
            ? Ok(ApiResponse<UsuarioDto>.Ok(result, "Técnico encontrado."))
            : NotFound(ApiResponse<object>.Fail("Técnico no encontrado."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Create([FromBody] UsuarioDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Datos inválidos.", validation.Errors.Select(e => e.ErrorMessage)));

        var id = await tecnicoService.CreateTecnicoAsync(dto);
        dto.Id = id;
        return CreatedAtAction(nameof(GetById), new { id },
            ApiResponse<UsuarioDto>.Ok(dto, "Técnico creado correctamente."));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Update([FromBody] UsuarioDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Datos inválidos.", validation.Errors.Select(e => e.ErrorMessage)));

        await tecnicoService.UpdateTecnicoAsync(dto);
        return Ok(ApiResponse.Ok("Técnico actualizado correctamente."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await tecnicoService.DeleteTecnicoAsync(id);
        return Ok(ApiResponse.Ok("Técnico eliminado correctamente."));
    }

    // ── Carga masiva ──────────────────────────────────────────────────────────

    [HttpGet("template")]
    [ProducesResponseType(typeof(FileResult), 200)]
    public IActionResult DescargarPlantilla([FromQuery] string rol = "Tecnico")
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Usuarios");

        var headers = rol == "Tecnico"
            ? new[] { "Nombre *", "Email *", "CodigoDependencia", "EmailProgramador" }
            : new[] { "Nombre *", "Email *", "CodigoDependencia" };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#6F42C1");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        ws.Cell(2, 1).Value = "Juan Pérez";
        ws.Cell(2, 2).Value = "juan.perez@empresa.com";
        ws.Cell(2, 3).Value = "DEP01";
        if (rol == "Tecnico") ws.Cell(2, 4).Value = "programador@empresa.com";
        ws.Row(2).Style.Font.Italic = true;
        ws.Row(2).Style.Font.FontColor = XLColor.Gray;

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"plantilla_{rol.ToLower()}s.xlsx");
    }

    [HttpPost("preview-excel")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<List<UsuarioPreviewRow>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> PreviewExcel(IFormFile file, [FromQuery] string rol = "Tecnico")
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Debe adjuntar un archivo Excel."));

        var todasDeps = await dependenciaService.GetAllAsync();
        var depsPorCodigo = todasDeps.ToDictionary(
            d => d.Codigo.ToUpperInvariant(), d => d, StringComparer.OrdinalIgnoreCase);

        IEnumerable<UsuarioDto> programadores = [];
        if (rol == "Tecnico")
        {
            var todos = await tecnicoService.GetAllUsuariosAsync();
            programadores = todos.Where(u => u.Rol == "Programador").ToList();
        }

        var rows = new List<UsuarioPreviewRow>();
        using var stream = file.OpenReadStream();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.First();

        int fila = 2;
        while (true)
        {
            var row = ws.Row(fila);
            if (row.Cell(1).IsEmpty() && row.Cell(2).IsEmpty()) break;

            var preview = new UsuarioPreviewRow { Fila = fila, Rol = rol };
            var errores = new List<string>();

            preview.Nombre            = row.Cell(1).GetString().Trim();
            preview.Email             = row.Cell(2).GetString().Trim();
            preview.CodigoDependencia = row.Cell(3).GetString().Trim();
            if (rol == "Tecnico")
                preview.EmailProgramador = row.Cell(4).GetString().Trim();

            if (string.IsNullOrWhiteSpace(preview.Nombre))
                errores.Add("Nombre es requerido");

            if (string.IsNullOrWhiteSpace(preview.Email))
                errores.Add("Email es requerido");
            else if (!IsValidEmail(preview.Email))
                errores.Add("Email no tiene formato válido");
            else if (await userService.GetByEmailAsync(preview.Email) != null)
                errores.Add($"El email ya está registrado");

            if (!string.IsNullOrWhiteSpace(preview.CodigoDependencia))
            {
                if (depsPorCodigo.TryGetValue(preview.CodigoDependencia, out var dep))
                    preview.DependenciaId = dep.Id;
                else
                    errores.Add($"Dependencia '{preview.CodigoDependencia}' no encontrada");
            }

            if (rol == "Tecnico" && !string.IsNullOrWhiteSpace(preview.EmailProgramador))
            {
                var prog = programadores.FirstOrDefault(p =>
                    p.Email.Equals(preview.EmailProgramador, StringComparison.OrdinalIgnoreCase));
                if (prog == null)
                    errores.Add($"Programador '{preview.EmailProgramador}' no encontrado");
                else
                    preview.ProgramadorId = prog.Id;
            }

            preview.EsValido = errores.Count == 0;
            preview.Error    = errores.Count > 0 ? string.Join("; ", errores) : null;
            rows.Add(preview);
            fila++;
        }

        return Ok(ApiResponse<List<UsuarioPreviewRow>>.Ok(rows,
            $"{rows.Count} fila(s): {rows.Count(r => r.EsValido)} válidas, {rows.Count(r => !r.EsValido)} con error."));
    }

    [HttpPost("bulk")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> BulkCreate([FromBody] List<UsuarioDto> items)
    {
        if (items == null || items.Count == 0)
            return BadRequest(ApiResponse<object>.Fail("No se enviaron usuarios."));

        int creados = 0;
        foreach (var dto in items)
        {
            await tecnicoService.CreateTecnicoAsync(dto);
            creados++;
        }

        return Ok(ApiResponse<object>.Ok(new { creados }, $"{creados} usuario(s) cargado(s) correctamente."));
    }

    private static bool IsValidEmail(string email)
    {
        try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email; }
        catch { return false; }
    }
}
