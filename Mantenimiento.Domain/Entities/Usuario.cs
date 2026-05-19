using Mantenimiento.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Mantenimiento.Domain.Entities;

public class Usuario : IdentityUser<Guid>
{
    public string Nombre { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public Guid? DependenciaId { get; set; }
    public Guid? ProgramadorId { get; set; }

    public Dependencia? Dependencia { get; set; }
    public Usuario? Programador { get; set; }
}
