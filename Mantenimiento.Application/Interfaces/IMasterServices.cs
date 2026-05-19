using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Application.Interfaces;

public interface IDependenciaService
{
    Task<IEnumerable<DependenciaDto>> GetAllAsync();
    Task<DependenciaDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(DependenciaDto dto);
    Task UpdateAsync(DependenciaDto dto);
    Task DeleteAsync(Guid id);
}

public interface IMaterialService
{
    Task<IEnumerable<MaterialDto>> GetAllAsync();
    Task<MaterialDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(MaterialDto dto);
    Task UpdateAsync(MaterialDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<Result> AjustarStockAsync(Guid id, int ajuste);
}

public interface ITecnicoService
{
    Task<IEnumerable<UsuarioDto>> GetAllTecnicosAsync();
    Task<IEnumerable<UsuarioDto>> GetAllUsuariosAsync();
    Task<UsuarioDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateTecnicoAsync(UsuarioDto dto);
    Task UpdateTecnicoAsync(UsuarioDto dto);
    Task DeleteTecnicoAsync(Guid id);
}
