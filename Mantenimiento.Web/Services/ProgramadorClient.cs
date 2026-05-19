using System.Net.Http.Json;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Web.Services;

public class ProgramadorClient(HttpClient http)
{
    // ── Técnicos a cargo ─────────────────────────────────────────────────────
    public async Task<List<UsuarioDto>> GetTecnicosAsync()
    {
        var r = await http.GetFromJsonAsync<ApiResponse<IEnumerable<UsuarioDto>>>("api/v1/programador/tecnicos", ApiJsonOptions.Default);
        return r?.Data?.ToList() ?? [];
    }

    // ── Órdenes ───────────────────────────────────────────────────────────────
    public async Task<List<OrdenTrabajoDto>> GetOrdenesAsync()
    {
        var r = await http.GetFromJsonAsync<ApiResponse<IEnumerable<OrdenTrabajoDto>>>("api/v1/programador/ordenes", ApiJsonOptions.Default);
        return r?.Data?.ToList() ?? [];
    }

    public async Task<byte[]?> ExportarOrdenesExcelAsync()
    {
        var response = await http.GetAsync("api/v1/programador/ordenes/export");
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }

    // ── Notificaciones ────────────────────────────────────────────────────────
    public async Task<List<NotificacionDto>> GetNotificacionesAsync(bool soloNoLeidas = false)
    {
        var url = $"api/v1/programador/notificaciones?soloNoLeidas={soloNoLeidas}";
        var r = await http.GetFromJsonAsync<ApiResponse<IEnumerable<NotificacionDto>>>(url, ApiJsonOptions.Default);
        return r?.Data?.ToList() ?? [];
    }

    public async Task MarcarLeidaAsync(Guid id)
    {
        await http.PutAsync($"api/v1/programador/notificaciones/{id}/leida", null);
    }

    public async Task MarcarTodasLeidasAsync()
    {
        await http.PutAsync("api/v1/programador/notificaciones/leer-todas", null);
    }

    // ── Aprobación de materiales ──────────────────────────────────────────────
    public async Task<Result> AprobarMaterialesAsync(AprobacionMaterialRequest request)
    {
        var resp = await http.PostAsJsonAsync("api/v1/programador/aprobar-materiales", request, ApiJsonOptions.Default);
        return resp.IsSuccessStatusCode ? Result.Success() : Result.Failure(await resp.Content.ReadAsStringAsync());
    }

    // ── Horarios ──────────────────────────────────────────────────────────────
    public async Task<List<TurnoHorarioDto>> GetTurnosAsync(Guid tecnicoId, DateOnly desde, DateOnly hasta)
    {
        var url = $"api/v1/programador/turnos/{tecnicoId}?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        var r = await http.GetFromJsonAsync<ApiResponse<IEnumerable<TurnoHorarioDto>>>(url, ApiJsonOptions.Default);
        return r?.Data?.ToList() ?? [];
    }

    public async Task<List<TurnoHorarioDto>> SugerirHorarioAsync(SugerirHorarioRequest request)
    {
        var resp = await http.PostAsJsonAsync("api/v1/programador/turnos/sugerir", request, ApiJsonOptions.Default);
        if (!resp.IsSuccessStatusCode) return [];
        var r = await resp.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<TurnoHorarioDto>>>(ApiJsonOptions.Default);
        return r?.Data?.ToList() ?? [];
    }

    public async Task<Result> GuardarTurnosAsync(GuardarTurnosRequest request)
    {
        var resp = await http.PostAsJsonAsync("api/v1/programador/turnos", request, ApiJsonOptions.Default);
        return resp.IsSuccessStatusCode ? Result.Success() : Result.Failure(await resp.Content.ReadAsStringAsync());
    }

    public async Task<Result> EliminarTurnoAsync(Guid turnoId)
    {
        var resp = await http.DeleteAsync($"api/v1/programador/turnos/{turnoId}");
        return resp.IsSuccessStatusCode ? Result.Success() : Result.Failure(await resp.Content.ReadAsStringAsync());
    }

    // ── Configuración empresa ─────────────────────────────────────────────────
    public async Task<ConfiguracionEmpresaDto?> GetConfiguracionAsync()
    {
        var r = await http.GetFromJsonAsync<ApiResponse<ConfiguracionEmpresaDto>>("api/v1/programador/configuracion", ApiJsonOptions.Default);
        return r?.Data;
    }

    public async Task<Result> ActualizarConfiguracionAsync(ConfiguracionEmpresaDto dto)
    {
        var resp = await http.PutAsJsonAsync("api/v1/programador/configuracion", dto, ApiJsonOptions.Default);
        return resp.IsSuccessStatusCode ? Result.Success() : Result.Failure(await resp.Content.ReadAsStringAsync());
    }

    // ── Recomendaciones de horario ────────────────────────────────────────────
    public async Task<List<RecomendacionHorarioDto>> GetRecomendacionesAsync()
    {
        var r = await http.GetFromJsonAsync<ApiResponse<IEnumerable<RecomendacionHorarioDto>>>("api/v1/programador/recomendaciones", ApiJsonOptions.Default);
        return r?.Data?.ToList() ?? [];
    }

    public async Task<Result> CrearRecomendacionAsync(RecomendacionHorarioDto dto)
    {
        var resp = await http.PostAsJsonAsync("api/v1/programador/recomendaciones", dto, ApiJsonOptions.Default);
        return resp.IsSuccessStatusCode ? Result.Success() : Result.Failure(await resp.Content.ReadAsStringAsync());
    }

    public async Task<Result> ActualizarRecomendacionAsync(RecomendacionHorarioDto dto)
    {
        var resp = await http.PutAsJsonAsync($"api/v1/programador/recomendaciones/{dto.Id}", dto, ApiJsonOptions.Default);
        return resp.IsSuccessStatusCode ? Result.Success() : Result.Failure(await resp.Content.ReadAsStringAsync());
    }

    public async Task<Result> EliminarRecomendacionAsync(Guid id)
    {
        var resp = await http.DeleteAsync($"api/v1/programador/recomendaciones/{id}");
        return resp.IsSuccessStatusCode ? Result.Success() : Result.Failure(await resp.Content.ReadAsStringAsync());
    }
}
