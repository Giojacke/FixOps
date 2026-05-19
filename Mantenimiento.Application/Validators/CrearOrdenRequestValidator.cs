using FluentValidation;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Application.Validators;

public class CrearOrdenRequestValidator : AbstractValidator<CrearOrdenRequest>
{
    public CrearOrdenRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MinimumLength(10).WithMessage("La descripción debe tener al menos 10 caracteres.");

        RuleFor(x => x.IdSolicitante)
            .NotEmpty().WithMessage("El ID del solicitante es obligatorio.");

        RuleFor(x => x.IdDependencia)
            .NotEmpty().WithMessage("El ID de la dependencia es obligatorio.");
            
        RuleFor(x => x.Urgencia)
            .IsInEnum().WithMessage("El nivel de urgencia no es válido.");
    }
}
