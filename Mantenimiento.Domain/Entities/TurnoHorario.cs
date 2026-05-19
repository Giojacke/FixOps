namespace Mantenimiento.Domain.Entities;

public class TurnoHorario
{
    public Guid Id { get; set; }
    public Guid TecnicoId { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public bool IncluyeAlmuerzo { get; set; }

    public Usuario Tecnico { get; set; } = null!;
}
