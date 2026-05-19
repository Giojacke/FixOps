namespace Mantenimiento.Domain.Entities;

public class Dependencia
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Regional { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string JefeEmail       { get; set; } = string.Empty;
    public string NombreContacto  { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Usuario> Personal { get; set; } = new List<Usuario>();
}
