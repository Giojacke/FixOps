using System.Net.Http.Json;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Domain.Enums;

namespace Mantenimiento.Web.Services;

public class OrdenTrabajoClient(HttpClient httpClient)
{
    public async Task<IEnumerable<OrdenTrabajoDto>> GetAllAsync(
        int page = 1, int pageSize = 100,
        string? estado = null, string? urgencia = null,
        string? folio = null, DateTime? desde = null, DateTime? hasta = null)
    {
        var sb = new System.Text.StringBuilder($"api/v1/orders?page={page}&pageSize={pageSize}");
        if (!string.IsNullOrEmpty(estado))   sb.Append($"&estado={estado}");
        if (!string.IsNullOrEmpty(urgencia)) sb.Append($"&urgencia={urgencia}");
        if (!string.IsNullOrEmpty(folio))    sb.Append($"&folio={Uri.EscapeDataString(folio)}");
        if (desde.HasValue)                  sb.Append($"&desde={desde.Value:yyyy-MM-dd}");
        if (hasta.HasValue)                  sb.Append($"&hasta={hasta.Value:yyyy-MM-dd}");
        var r = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<OrdenTrabajoDto>>>(sb.ToString(), ApiJsonOptions.Default);
        return r?.Data?.Items ?? [];
    }

    public async Task<IEnumerable<OrdenTrabajoDto>> GetByTecnicoAsync(Guid tecnicoId, int page = 1, int pageSize = 50)
    {
        var response = await httpClient.GetAsync($"api/v1/orders/technician/{tecnicoId}?page={page}&pageSize={pageSize}");
        if (!response.IsSuccessStatusCode) return [];
        var r = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<OrdenTrabajoDto>>>(ApiJsonOptions.Default);
        return r?.Data?.Items ?? [];
    }

    public async Task<Result<OrdenTrabajoDto>> RegistrarOrdenAsync(CrearOrdenRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/orders", request, ApiJsonOptions.Default);
        if (response.IsSuccessStatusCode)
        {
            var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<OrdenTrabajoDto>>(ApiJsonOptions.Default);
            return Result<OrdenTrabajoDto>.Success(envelope!.Data!);
        }
        return Result<OrdenTrabajoDto>.Failure(await response.Content.ReadAsStringAsync());
    }

    public async Task<Result<OrdenTrabajoDto>> GetOrdenByIdAsync(Guid id)
    {
        var response = await httpClient.GetAsync($"api/v1/orders/{id}");
        if (response.IsSuccessStatusCode)
        {
            var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<OrdenTrabajoDto>>(ApiJsonOptions.Default);
            return Result<OrdenTrabajoDto>.Success(envelope!.Data!);
        }
        return Result<OrdenTrabajoDto>.Failure(await response.Content.ReadAsStringAsync());
    }

    public async Task<Result> AgregarOperacionAsync(Guid ordenId, AgregarOperacionRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"api/v1/orders/{ordenId}/operations", request, ApiJsonOptions.Default);
        return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
    }

    public async Task<Result> ActualizarEstadoOrdenAsync(Guid ordenId, EstadoOrden nuevoEstado)
    {
        var response = await httpClient.PutAsJsonAsync($"api/v1/orders/{ordenId}/status", new { NuevoEstado = nuevoEstado }, ApiJsonOptions.Default);
        return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
    }

    public async Task<Result> ActualizarEstadoOperacionAsync(Guid operacionId, EstadoOperacion nuevoEstado)
    {
        var response = await httpClient.PutAsJsonAsync($"api/v1/orders/operations/{operacionId}/status", new { NuevoEstado = nuevoEstado }, ApiJsonOptions.Default);
        return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
    }

    public async Task<Result> EliminarOperacionAsync(Guid operacionId)
    {
        var response = await httpClient.DeleteAsync($"api/v1/orders/operations/{operacionId}");
        return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
    }

    public async Task<Result> EditarOperacionAsync(Guid operacionId, EditarOperacionRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/v1/orders/operations/{operacionId}", request, ApiJsonOptions.Default);
        return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
    }

    public async Task<Result> RegistrarActividadAsync(Guid operacionId, RegistrarActividadRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"api/v1/orders/operations/{operacionId}/activity", request, ApiJsonOptions.Default);
        return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
    }
}

public class AgregarOperacionRequest { public int Numero { get; set; } public string Descripcion { get; set; } = string.Empty; public int HorasHombre { get; set; } }
public class EditarOperacionRequest   { public int Numero { get; set; } public string Descripcion { get; set; } = string.Empty; public int HorasHombre { get; set; } }
