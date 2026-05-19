using System.Net.Http.Json;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Web.Services;

public class ConfiguracionClient(HttpClient http)
{
    public async Task<ConfiguracionCorreoDto?> GetCorreoAsync()
    {
        var r = await http.GetFromJsonAsync<ApiResponse<ConfiguracionCorreoDto>>("api/v1/configuracion/correo", ApiJsonOptions.Default);
        return r?.Data;
    }

    public async Task<Result> UpdateCorreoAsync(ConfiguracionCorreoDto dto)
    {
        var resp = await http.PutAsJsonAsync("api/v1/configuracion/correo", dto, ApiJsonOptions.Default);
        return resp.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(await resp.Content.ReadAsStringAsync());
    }

    public async Task<Result> EnviarPruebaAsync(string emailDestino)
    {
        var resp = await http.PostAsJsonAsync(
            "api/v1/configuracion/correo/prueba",
            new { emailDestino },
            ApiJsonOptions.Default);
        if (resp.IsSuccessStatusCode) return Result.Success();
        var body = await resp.Content.ReadAsStringAsync();
        try
        {
            var err = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<object>>(body, ApiJsonOptions.Default);
            return Result.Failure(err?.Message ?? body);
        }
        catch { return Result.Failure(body); }
    }
}
