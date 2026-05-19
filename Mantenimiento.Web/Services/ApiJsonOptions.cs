using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mantenimiento.Web.Services;

public static class ApiJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
