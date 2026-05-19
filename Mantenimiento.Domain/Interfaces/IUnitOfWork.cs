using Mantenimiento.Domain.Entities;

namespace Mantenimiento.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IOrdenTrabajoRepository OrdenesTrabajo { get; }
    IRepository<Usuario> Usuarios { get; }
    IRepository<Dependencia> Dependencias { get; }
    IRepository<Material> Materiales { get; }
    IRepository<Operacion> Operaciones { get; }
    IRepository<SolicitudMaterial> SolicitudesMateriales { get; }
    IEncuestaRepository Encuestas { get; }
    IRepository<Notificacion> Notificaciones { get; }
    IRepository<TurnoHorario> Turnos { get; }
    IRepository<ConfiguracionEmpresa>  ConfiguracionEmpresa  { get; }
    IRepository<RecomendacionHorario>  Recomendaciones       { get; }
    IRepository<RegistroAuditoria>    Auditorias           { get; }
    IRepository<ConfiguracionCorreo>  ConfiguracionCorreo  { get; }
    IRepository<CorreoEncolado>       CorreosEncolados     { get; }

    Task<int> SaveChangesAsync();
}
