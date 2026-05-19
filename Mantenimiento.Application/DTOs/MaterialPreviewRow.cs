namespace Mantenimiento.Application.DTOs;

public class MaterialPreviewRow
{
    public int Fila { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoMaterial { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool EsValido { get; set; }
    public string? Error { get; set; }
}
