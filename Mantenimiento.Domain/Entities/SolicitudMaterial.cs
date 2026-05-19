namespace Mantenimiento.Domain.Entities;

public class SolicitudMaterial
{
    public Guid Id                { get; set; }
    public Guid OperacionId       { get; set; }
    public Guid? MaterialId       { get; set; }
    public string NombreMaterial  { get; set; } = string.Empty;
    public int Cantidad           { get; set; }
    public bool EsPersonalizado   { get; set; }
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    public Operacion Operacion    { get; set; } = null!;
    public Material? Material     { get; set; }
}
