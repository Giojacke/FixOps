namespace Mantenimiento.Domain.Entities;

public class ConfiguracionEmpresa
{
    public Guid Id { get; set; }
    public bool AlmuerzoPagadoEnJornada { get; set; } = true;
    public int HorasDiariasEfectivas { get; set; } = 7;
    public int HorasSemanalesMaximas { get; set; } = 42;
    public int HorasExtrasMaximas    { get; set; } = 8;
    public TimeOnly HoraInicioDefault { get; set; } = new TimeOnly(7, 0);
}
