using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Application.Interfaces;

public interface IUserService
{
    Task<UsuarioDto?> GetByIdAsync(Guid id);
    Task<UsuarioDto?> GetByEmailAsync(string email);
    Task<IEnumerable<UsuarioDto>> GetByRoleAsync(string role);
    Task<Guid> CreateAsync(UsuarioDto dto, string temporaryPassword);
    Task UpdateAsync(UsuarioDto dto);
    Task DeleteAsync(Guid id);
    Task AssignRoleAsync(Guid userId, string role);
}
