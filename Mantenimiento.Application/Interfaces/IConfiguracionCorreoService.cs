using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Application.Interfaces;

public interface IConfiguracionCorreoService
{
    Task<ConfiguracionCorreoDto> GetAsync();
    Task<Result>                 UpdateAsync(ConfiguracionCorreoDto dto);
    Task<Result>                 EnviarPruebaAsync(string emailDestino);
}
