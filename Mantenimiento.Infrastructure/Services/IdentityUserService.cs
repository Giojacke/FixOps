using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Mantenimiento.Infrastructure.Services;

public class IdentityUserService(UserManager<Usuario> userManager) : IUserService
{
    public async Task<UsuarioDto?> GetByIdAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        return user is null ? null : ToDto(user);
    }

    public async Task<UsuarioDto?> GetByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is null ? null : ToDto(user);
    }

    public async Task<IEnumerable<UsuarioDto>> GetByRoleAsync(string role)
    {
        var users = await userManager.GetUsersInRoleAsync(role);
        return users.Select(ToDto);
    }

    public async Task<Guid> CreateAsync(UsuarioDto dto, string temporaryPassword)
    {
        var user = new Usuario
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            UserName = dto.Email,
            Email = dto.Email,
            Nombre = dto.Nombre,
            Rol = ParseRole(dto.Rol),
            DependenciaId = dto.DependenciaId
        };

        var result = await userManager.CreateAsync(user, temporaryPassword);
        EnsureSuccess(result);

        if (!string.IsNullOrWhiteSpace(dto.Rol))
        {
            var roleResult = await userManager.AddToRoleAsync(user, dto.Rol);
            EnsureSuccess(roleResult);
        }

        return user.Id;
    }

    public async Task UpdateAsync(UsuarioDto dto)
    {
        var user = await userManager.FindByIdAsync(dto.Id.ToString());
        if (user is null) return;

        user.Nombre       = dto.Nombre;
        user.Email        = dto.Email;
        user.UserName     = dto.Email;
        user.Rol          = ParseRole(dto.Rol);
        user.DependenciaId  = dto.DependenciaId;
        user.ProgramadorId  = dto.ProgramadorId;

        var result = await userManager.UpdateAsync(user);
        EnsureSuccess(result);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return;

        var result = await userManager.DeleteAsync(user);
        EnsureSuccess(result);
    }

    public async Task AssignRoleAsync(Guid userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return;

        if (await userManager.IsInRoleAsync(user, role)) return;

        var result = await userManager.AddToRoleAsync(user, role);
        EnsureSuccess(result);
    }

    private static UsuarioDto ToDto(Usuario user) => new()
    {
        Id                = user.Id,
        Nombre            = user.Nombre,
        Email             = user.Email ?? string.Empty,
        Rol               = user.Rol.ToString(),
        DependenciaId     = user.DependenciaId,
        DependenciaNombre = string.Empty,
        ProgramadorId     = user.ProgramadorId,
        ProgramadorNombre = user.Programador?.Nombre ?? string.Empty
    };

    private static RolUsuario ParseRole(string role) =>
        Enum.TryParse<RolUsuario>(role, true, out var parsedRole) ? parsedRole : default;

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(e => e.Description)));
    }
}
