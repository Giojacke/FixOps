using System.Net.Http.Json;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Web.Services;

public class EncuestaClient(HttpClient httpClient)
{
    public async Task<bool> RegistrarAsync(CrearEncuestaRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/surveys", request, ApiJsonOptions.Default);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<EncuestaDto>> GetResultadosAsync(DateTime? desde = null, DateTime? hasta = null, int page = 1, int pageSize = 50)
    {
        var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (desde.HasValue) query.Add($"desde={desde.Value:yyyy-MM-dd}");
        if (hasta.HasValue) query.Add($"hasta={hasta.Value:yyyy-MM-dd}");
        var url = "api/v1/surveys/results?" + string.Join("&", query);

        var r = await httpClient.GetFromJsonAsync<ApiResponse<PagedResult<EncuestaDto>>>(url, ApiJsonOptions.Default);
        return r?.Data?.Items ?? [];
    }

    public async Task<MetricasDto?> GetMetricasAsync()
    {
        var r = await httpClient.GetFromJsonAsync<ApiResponse<MetricasDto>>("api/v1/surveys/metrics", ApiJsonOptions.Default);
        return r?.Data;
    }
}
