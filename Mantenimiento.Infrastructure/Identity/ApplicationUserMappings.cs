using Mantenimiento.Domain.Entities;

namespace Mantenimiento.Infrastructure.Identity;

public static class ApplicationUserMappings
{
    public static Usuario ToDomain(this ApplicationUser identityUser) => new()
    {
        Id = identityUser.Id,
        Nombre = identityUser.Nombre,
        Email = identityUser.Email,
        Rol = identityUser.Rol,
        DependenciaId = identityUser.DependenciaId
    };

    public static ApplicationUser ToIdentity(this Usuario usuario) => new()
    {
        Id = usuario.Id,
        UserName = usuario.Email,
        Email = usuario.Email,
        Nombre = usuario.Nombre,
        Rol = usuario.Rol,
        DependenciaId = usuario.DependenciaId
    };
}
