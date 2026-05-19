using System.Net.Http.Json;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using MaterialPreviewRow = Mantenimiento.Application.DTOs.MaterialPreviewRow;

namespace Mantenimiento.Web.Services;

public class DependenciaClient(HttpClient httpClient)
{
    public async Task<IEnumerable<DependenciaDto>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var r = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<DependenciaDto>>>(
            $"api/v1/dependencies?page={page}&pageSize={pageSize}", ApiJsonOptions.Default);
        return r?.Data?.Items ?? [];
    }

    public async Task CreateAsync(DependenciaDto dto) =>
        await httpClient.PostAsJsonAsync("api/v1/dependencies", dto, ApiJsonOptions.Default);

    public async Task UpdateAsync(DependenciaDto dto) =>
        await httpClient.PutAsJsonAsync("api/v1/dependencies", dto, ApiJsonOptions.Default);

    public async Task DeleteAsync(Guid id) =>
        await httpClient.DeleteAsync($"api/v1/dependencies/{id}");

    public async Task<List<DependenciaPreviewRow>?> PreviewExcelAsync(Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        var response = await httpClient.PostAsync("api/v1/dependencies/preview-excel", content);
        if (!response.IsSuccessStatusCode) return null;
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<List<DependenciaPreviewRow>>>(ApiJsonOptions.Default);
        return envelope?.Data;
    }

    public async Task<int> BulkCreateAsync(List<DependenciaDto> items)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/dependencies/bulk", items, ApiJsonOptions.Default);
        if (!response.IsSuccessStatusCode) return 0;
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<BulkResult>>(ApiJsonOptions.Default);
        return envelope?.Data?.Creados ?? 0;
    }

    public async Task<byte[]?> DescargarPlantillaAsync()
    {
        var response = await httpClient.GetAsync("api/v1/dependencies/template");
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }
}

public class MaterialClient(HttpClient httpClient)
{
    public async Task<IEnumerable<MaterialDto>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var r = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<MaterialDto>>>(
            $"api/v1/materials?page={page}&pageSize={pageSize}", ApiJsonOptions.Default);
        return r?.Data?.Items ?? [];
    }

    public async Task CreateAsync(MaterialDto dto) =>
        await httpClient.PostAsJsonAsync("api/v1/materials", dto, ApiJsonOptions.Default);

    public async Task UpdateAsync(MaterialDto dto) =>
        await httpClient.PutAsJsonAsync("api/v1/materials", dto, ApiJsonOptions.Default);

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"api/v1/materials/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<MaterialPreviewRow>?> PreviewExcelAsync(Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        var response = await httpClient.PostAsync("api/v1/materials/preview-excel", content);
        if (!response.IsSuccessStatusCode) return null;
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<List<MaterialPreviewRow>>>(ApiJsonOptions.Default);
        return envelope?.Data;
    }

    public async Task<int> BulkCreateAsync(List<MaterialDto> items)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/materials/bulk", items, ApiJsonOptions.Default);
        if (!response.IsSuccessStatusCode) return 0;
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<BulkResult>>(ApiJsonOptions.Default);
        return envelope?.Data?.Creados ?? 0;
    }

    public async Task<Result> AjustarStockAsync(Guid id, int ajuste)
    {
        var response = await httpClient.PatchAsJsonAsync(
            $"api/v1/materials/{id}/stock",
            new { ajuste },
            ApiJsonOptions.Default);
        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(await response.Content.ReadAsStringAsync());
    }

    public async Task<byte[]?> ExportarExcelAsync()
    {
        var response = await httpClient.GetAsync("api/v1/materials/export");
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<byte[]?> DescargarPlantillaAsync()
    {
        var response = await httpClient.GetAsync("api/v1/materials/template");
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }
}

public class UsuarioClient(HttpClient httpClient)
{
    public async Task<IEnumerable<UsuarioDto>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var r = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<UsuarioDto>>>(
            $"api/v1/technicians?page={page}&pageSize={pageSize}", ApiJsonOptions.Default);
        return r?.Data?.Items ?? [];
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllTecnicosAsync()
    {
        var all = await GetAllUsuariosAsync();
        return all.Where(u => u.Rol == "Tecnico");
    }

    public async Task CreateAsync(UsuarioDto dto) =>
        await httpClient.PostAsJsonAsync("api/v1/technicians", dto, ApiJsonOptions.Default);

    public async Task UpdateAsync(UsuarioDto dto) =>
        await httpClient.PutAsJsonAsync("api/v1/technicians", dto, ApiJsonOptions.Default);

    public async Task DeleteAsync(Guid id) =>
        await httpClient.DeleteAsync($"api/v1/technicians/{id}");

    public async Task<IEnumerable<UsuarioDto>> GetAllUsuariosAsync()
    {
        var r = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<UsuarioDto>>>(
            "api/v1/technicians?page=1&pageSize=500", ApiJsonOptions.Default);
        return r?.Data?.Items ?? [];
    }

    public async Task<IEnumerable<UsuarioDto>> GetProgramadoresAsync()
    {
        var all = await GetAllUsuariosAsync();
        return all.Where(u => u.Rol == "Programador");
    }

    public async Task<IEnumerable<UsuarioDto>> GetByRolAsync(string rol)
    {
        var all = await GetAllUsuariosAsync();
        return all.Where(u => u.Rol == rol);
    }

    public async Task<List<UsuarioPreviewRow>?> PreviewExcelAsync(Stream fileStream, string fileName, string rol)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        var response = await httpClient.PostAsync($"api/v1/technicians/preview-excel?rol={rol}", content);
        if (!response.IsSuccessStatusCode) return null;
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<List<UsuarioPreviewRow>>>(ApiJsonOptions.Default);
        return envelope?.Data;
    }

    public async Task<int> BulkCreateAsync(List<UsuarioDto> items)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/technicians/bulk", items, ApiJsonOptions.Default);
        if (!response.IsSuccessStatusCode) return 0;
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<BulkResult>>(ApiJsonOptions.Default);
        return envelope?.Data?.Creados ?? 0;
    }

    public async Task<byte[]?> DescargarPlantillaAsync(string rol)
    {
        var response = await httpClient.GetAsync($"api/v1/technicians/template?rol={rol}");
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/auth/login", request, ApiJsonOptions.Default);
        if (!response.IsSuccessStatusCode) return null;
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(ApiJsonOptions.Default);
        return envelope?.Data;
    }
}

public class LoginRequest  { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class LoginResponse { public string Token { get; set; } = string.Empty; public string Nombre { get; set; } = string.Empty; public string Rol { get; set; } = string.Empty; }

file record BulkResult(int Creados);
