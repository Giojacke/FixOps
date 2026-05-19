namespace Mantenimiento.Application.DTOs;

// ── DTOs originales de métricas de encuestas ──────────────────────────────
public record MetricaTecnicoDto(string NombreTecnico, double Promedio, int TotalEncuestas);
public record MetricaDependenciaDto(string Dependencia, double Promedio, int TotalEncuestas);
public record MetricasDto(IEnumerable<MetricaTecnicoDto> PorTecnico, IEnumerable<MetricaDependenciaDto> PorDependencia);

// ── DTOs del dashboard de métricas operativas ─────────────────────────────

public class DashboardDto
{
    public DashboardKpis              Kpis                { get; set; } = new();
    public List<EstadoCountDto>       OrdenesPorEstado    { get; set; } = [];
    public List<RegionalMetricaDto>   MetricasPorRegional { get; set; } = [];
    public List<TendenciaMensualDto>  Tendencia           { get; set; } = [];
    public List<string>               Regionales          { get; set; } = [];
}

public class DashboardKpis
{
    public int    TotalOrdenes        { get; set; }
    public double TiempoPromedioHoras { get; set; }
    public int    Pendientes          { get; set; }
    public int    EnProceso           { get; set; }
    public int    Finalizadas         { get; set; }
    public int    Canceladas          { get; set; }
    public int    OperacionesEnPausa  { get; set; }
}

public class EstadoCountDto
{
    public string Estado   { get; set; } = string.Empty;
    public int    Cantidad { get; set; }
}

public class RegionalMetricaDto
{
    public string Regional        { get; set; } = string.Empty;
    public int    CantidadOrdenes { get; set; }
    public double PromedioHoras   { get; set; }
}

public class TendenciaMensualDto
{
    public string Periodo     { get; set; } = string.Empty;
    public int    Creadas     { get; set; }
    public int    Finalizadas { get; set; }
}
