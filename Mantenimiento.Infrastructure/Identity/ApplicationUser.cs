using Mantenimiento.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Mantenimiento.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Nombre { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public Guid? DependenciaId { get; set; }
}
