using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;
using Mantenimiento.Application.Interfaces;
using Mantenimiento.Application.Mappings;
using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Enums;
using Mantenimiento.Domain.Interfaces;

namespace Mantenimiento.Application.Services;

public class DependenciaService(IUnitOfWork unitOfWork) : IDependenciaService
{
    public async Task<IEnumerable<DependenciaDto>> GetAllAsync()
    {
        var entities = await unitOfWork.Dependencias.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<DependenciaDto?> GetByIdAsync(Guid id)
    {
        var entity = await unitOfWork.Dependencias.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<Guid> CreateAsync(DependenciaDto dto)
    {
        var entity = new Dependencia
        {
            Id             = Guid.NewGuid(),
            Nombre         = dto.Nombre,
            Ubicacion      = dto.Ubicacion,
            Codigo         = dto.Codigo,
            Regional       = dto.Regional,
            Departamento   = dto.Departamento,
            JefeEmail      = dto.JefeEmail,
            NombreContacto = dto.NombreContacto
        };
        await unitOfWork.Dependencias.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(DependenciaDto dto)
    {
        var entity = await unitOfWork.Dependencias.GetByIdAsync(dto.Id);
        if (entity != null)
        {
            entity.Nombre = dto.Nombre;
            entity.Ubicacion = dto.Ubicacion;
            entity.Codigo = dto.Codigo;
            entity.Regional = dto.Regional;
            entity.Departamento = dto.Departamento;
            entity.JefeEmail      = dto.JefeEmail;
            entity.NombreContacto = dto.NombreContacto;
            unitOfWork.Dependencias.Update(entity);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await unitOfWork.Dependencias.GetByIdAsync(id);
        if (entity != null)
        {
            unitOfWork.Dependencias.Remove(entity);
            await unitOfWork.SaveChangesAsync();
        }
    }
}

public class MaterialService(IUnitOfWork unitOfWork) : IMaterialService
{
    public async Task<IEnumerable<MaterialDto>> GetAllAsync()
    {
        var entities = await unitOfWork.Materiales.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<MaterialDto?> GetByIdAsync(Guid id)
    {
        var entity = await unitOfWork.Materiales.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<Guid> CreateAsync(MaterialDto dto)
    {
        var entity = new Material
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre,
            TipoMaterial = dto.TipoMaterial,
            Descripcion = dto.Descripcion,
            PrecioUnitario = dto.PrecioUnitario,
            StockActual = dto.StockActual
        };
        await unitOfWork.Materiales.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(MaterialDto dto)
    {
        var entity = await unitOfWork.Materiales.GetByIdAsync(dto.Id);
        if (entity != null)
        {
            entity.Nombre = dto.Nombre;
            entity.TipoMaterial = dto.TipoMaterial;
            entity.Descripcion = dto.Descripcion;
            entity.PrecioUnitario = dto.PrecioUnitario;
            entity.StockActual = dto.StockActual;
            unitOfWork.Materiales.Update(entity);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await unitOfWork.Materiales.GetByIdAsync(id);
        if (entity == null) return false;

        var enUso = (await unitOfWork.Operaciones.GetAllAsync())
            .Any(o => o.MaterialesUsados.Any(m => m.Id == id));
        if (enUso) return false;

        unitOfWork.Materiales.Remove(entity);
        await unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Result> AjustarStockAsync(Guid id, int ajuste)
    {
        var entity = await unitOfWork.Materiales.GetByIdAsync(id);
        if (entity == null) return Result.Failure("Material no encontrado.");

        var nuevo = entity.StockActual + ajuste;
        if (nuevo < 0)
            return Result.Failure($"Stock insuficiente. Stock actual: {entity.StockActual}, ajuste: {ajuste}.");

        entity.StockActual = nuevo;
        unitOfWork.Materiales.Update(entity);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}

public class TecnicoService(
    IUnitOfWork unitOfWork,
    IUserService userService) : ITecnicoService
{
    public async Task<IEnumerable<UsuarioDto>> GetAllTecnicosAsync()
    {
        var entities = await unitOfWork.Usuarios.FindAsync(u => u.Rol == RolUsuario.Tecnico);
        return entities.Select(e => e.ToDto());
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllUsuariosAsync()
    {
        var entities = await unitOfWork.Usuarios.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<UsuarioDto?> GetByIdAsync(Guid id)
    {
        var entity = await unitOfWork.Usuarios.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<Guid> CreateTecnicoAsync(UsuarioDto dto)
    {
        var existingUser = await userService.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return existingUser.Id;

        const string tempPassword = "Admin123!";
        var userToCreate = new UsuarioDto
        {
            Id            = Guid.NewGuid(),
            Nombre        = dto.Nombre,
            Email         = dto.Email,
            Rol           = dto.Rol,
            DependenciaId = dto.DependenciaId,
            ProgramadorId = dto.ProgramadorId
        };
        return await userService.CreateAsync(userToCreate, tempPassword);
    }

    public async Task UpdateTecnicoAsync(UsuarioDto dto)
    {
        var entity = await userService.GetByIdAsync(dto.Id);
        if (entity != null)
            await userService.UpdateAsync(dto);
    }

    public async Task DeleteTecnicoAsync(Guid id)
    {
        var entity = await userService.GetByIdAsync(id);
        if (entity != null)
        {
            await userService.DeleteAsync(id);
        }
    }
}

