namespace Mantenimiento.Domain.Entities;

public class RegistroAuditoria
{
    public Guid     Id              { get; set; }
    public string   TipoEntidad     { get; set; } = string.Empty; // "Orden" | "Operacion"
    public Guid     EntidadId       { get; set; }
    public string   ResumenEntidad  { get; set; } = string.Empty; // folio o "Op. 10 – descripción"
    public string   Accion          { get; set; } = string.Empty; // "Eliminacion" | "Cancelacion"
    public DateTime FechaHora       { get; set; }
    public Guid?    UsuarioId       { get; set; }
    public string   UsuarioNombre   { get; set; } = string.Empty;
    public string   UsuarioEmail    { get; set; } = string.Empty;
}
