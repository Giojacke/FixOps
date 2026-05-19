namespace Mantenimiento.Application.DTOs;

public class AjustarStockRequest
{
    public int     Ajuste { get; set; }   // positivo = entrada, negativo = salida
    public string? Motivo { get; set; }
}

public class SolicitudMaterialDto
{
    public Guid   Id             { get; set; }
    public string NombreMaterial { get; set; } = string.Empty;
    public int    Cantidad       { get; set; }
    public bool   EsPersonalizado { get; set; }
    public DateTime FechaSolicitud { get; set; }
}

public class DependenciaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Regional { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string JefeEmail      { get; set; } = string.Empty;
    public string NombreContacto { get; set; } = string.Empty;
}

public class MaterialDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoMaterial { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int StockActual { get; set; }
}

public class UsuarioDto
{
    public Guid    Id                { get; set; }
    public string  Nombre            { get; set; } = string.Empty;
    public string  Email             { get; set; } = string.Empty;
    public string  Rol               { get; set; } = string.Empty;
    public Guid?   DependenciaId     { get; set; }
    public string  DependenciaNombre { get; set; } = string.Empty;
    public Guid?   ProgramadorId     { get; set; }
    public string  ProgramadorNombre { get; set; } = string.Empty;
}

public class DependenciaPreviewRow
{
    public int     Fila           { get; set; }
    public string  Nombre         { get; set; } = string.Empty;
    public string  Codigo         { get; set; } = string.Empty;
    public string  Regional       { get; set; } = string.Empty;
    public string  Departamento   { get; set; } = string.Empty;
    public string  Ubicacion      { get; set; } = string.Empty;
    public string  NombreContacto { get; set; } = string.Empty;
    public string  JefeEmail      { get; set; } = string.Empty;
    public bool    EsValido       { get; set; }
    public string? Error          { get; set; }
}

public class UsuarioPreviewRow
{
    public int     Fila              { get; set; }
    public string  Nombre            { get; set; } = string.Empty;
    public string  Email             { get; set; } = string.Empty;
    public string  CodigoDependencia { get; set; } = string.Empty;
    public string  EmailProgramador  { get; set; } = string.Empty;
    public string  Rol               { get; set; } = string.Empty;
    public Guid?   DependenciaId     { get; set; }
    public Guid?   ProgramadorId     { get; set; }
    public bool    EsValido          { get; set; }
    public string? Error             { get; set; }
}
