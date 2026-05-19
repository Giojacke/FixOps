using FluentValidation;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Application.Validators;

public class DependenciaDtoValidator : AbstractValidator<DependenciaDto>
{
    public DependenciaDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Regional).NotEmpty();
        RuleFor(x => x.Departamento).NotEmpty();
    }
}

public class MaterialDtoValidator : AbstractValidator<MaterialDto>
{
    public MaterialDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.TipoMaterial).NotEmpty();
        RuleFor(x => x.PrecioUnitario).GreaterThan(0);
        RuleFor(x => x.StockActual).GreaterThanOrEqualTo(0);
    }
}

public class UsuarioDtoValidator : AbstractValidator<UsuarioDto>
{
    public UsuarioDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Rol).NotEmpty();
    }
}
