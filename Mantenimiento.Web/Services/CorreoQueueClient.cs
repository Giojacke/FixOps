using System.Net.Http.Json;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Web.Services;

public class CorreoQueueClient(HttpClient http)
{
    public async Task<IEnumerable<CorreoEncoladoDto>> GetAllAsync(CorreoQueueFilter filter)
    {
        var url = $"api/v1/correo-queue?page={filter.Page}&pageSize={filter.PageSize}";
        if (!string.IsNullOrWhiteSpace(filter.Estado))     url += $"&estado={filter.Estado}";
        if (!string.IsNullOrWhiteSpace(filter.TipoCorreo)) url += $"&tipoCorreo={filter.TipoCorreo}";

        var r = await http.GetFromJsonAsync<ApiResponse<IEnumerable<CorreoEncoladoDto>>>(url, ApiJsonOptions.Default);
        return r?.Data ?? [];
    }

    public async Task<CorreoQueueStatsDto?> GetStatsAsync()
    {
        var r = await http.GetFromJsonAsync<ApiResponse<CorreoQueueStatsDto>>("api/v1/correo-queue/stats", ApiJsonOptions.Default);
        return r?.Data;
    }

    public async Task<Result> ReintentarAsync(Guid id)
    {
        var resp = await http.PostAsync($"api/v1/correo-queue/{id}/reintentar", null);
        return resp.IsSuccessStatusCode ? Result.Success() : Result.Failure(await resp.Content.ReadAsStringAsync());
    }

    public async Task<Result> ReintentarFallidosAsync()
    {
        var resp = await http.PostAsync("api/v1/correo-queue/reintentar-fallidos", null);
        return resp.IsSuccessStatusCode ? Result.Success() : Result.Failure(await resp.Content.ReadAsStringAsync());
    }
}
