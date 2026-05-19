using FluentValidation;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Application.Validators;

public class CrearEncuestaRequestValidator : AbstractValidator<CrearEncuestaRequest>
{
    public CrearEncuestaRequestValidator()
    {
        RuleFor(x => x.OrdenTrabajoId).NotEmpty();
        RuleFor(x => x.PuntajeAtencion).InclusiveBetween(1, 5);
        RuleFor(x => x.PuntajeServicio).InclusiveBetween(1, 5);
        RuleFor(x => x.PuntajeTiempo).InclusiveBetween(1, 5);
        RuleFor(x => x.Comentarios).MaximumLength(500);
    }
}
