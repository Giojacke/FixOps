namespace Mantenimiento.Domain.Entities;

public class Material
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoMaterial { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int StockActual { get; set; }
}
