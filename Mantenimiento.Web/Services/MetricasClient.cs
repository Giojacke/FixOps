using System.Net.Http.Json;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Web.Services;

public class MetricasClient(HttpClient http)
{
    public async Task<DashboardDto?> GetMetricasAsync(string? regional = null, DateTime? desde = null, DateTime? hasta = null)
    {
        var qs = new List<string>();
        if (!string.IsNullOrEmpty(regional)) qs.Add($"regional={Uri.EscapeDataString(regional)}");
        if (desde.HasValue)  qs.Add($"desde={desde.Value:yyyy-MM-dd}");
        if (hasta.HasValue)  qs.Add($"hasta={hasta.Value:yyyy-MM-dd}");
        var url = "api/v1/metricas" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
        var r = await http.GetFromJsonAsync<ApiResponse<DashboardDto>>(url, ApiJsonOptions.Default);
        return r?.Data;
    }
}
