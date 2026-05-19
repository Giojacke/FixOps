using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Interfaces;
using Mantenimiento.Infrastructure.Persistence.Repositories;

namespace Mantenimiento.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly MantenimientoDbContext _context;
    private IOrdenTrabajoRepository? _ordenesTrabajo;
    private IRepository<Usuario>? _usuarios;
    private IRepository<Dependencia>? _dependencias;
    private IRepository<Material>? _materiales;
    private IRepository<Operacion>? _operaciones;
    private IRepository<SolicitudMaterial>? _solicitudesMateriales;
    private IEncuestaRepository? _encuestas;
    private IRepository<Notificacion>? _notificaciones;
    private IRepository<TurnoHorario>? _turnos;
    private IRepository<ConfiguracionEmpresa>?  _configuracionEmpresa;
    private IRepository<RecomendacionHorario>?  _recomendaciones;
    private IRepository<RegistroAuditoria>?    _auditorias;
    private IRepository<ConfiguracionCorreo>?  _configuracionCorreo;
    private IRepository<CorreoEncolado>?       _correosEncolados;

    public UnitOfWork(MantenimientoDbContext context)
    {
        _context = context;
    }

    public IOrdenTrabajoRepository OrdenesTrabajo => _ordenesTrabajo ??= new OrdenTrabajoRepository(_context);
    public IRepository<Usuario> Usuarios => _usuarios ??= new EfRepository<Usuario>(_context);
    public IRepository<Dependencia> Dependencias => _dependencias ??= new EfRepository<Dependencia>(_context);
    public IRepository<Material> Materiales => _materiales ??= new EfRepository<Material>(_context);
    public IRepository<Operacion> Operaciones => _operaciones ??= new EfRepository<Operacion>(_context);
    public IRepository<SolicitudMaterial> SolicitudesMateriales => _solicitudesMateriales ??= new EfRepository<SolicitudMaterial>(_context);
    public IEncuestaRepository Encuestas => _encuestas ??= new EncuestaRepository(_context);
    public IRepository<Notificacion> Notificaciones => _notificaciones ??= new NotificacionRepository(_context);
    public IRepository<TurnoHorario> Turnos => _turnos ??= new TurnoHorarioRepository(_context);
    public IRepository<ConfiguracionEmpresa>  ConfiguracionEmpresa  => _configuracionEmpresa  ??= new EfRepository<ConfiguracionEmpresa>(_context);
    public IRepository<RecomendacionHorario>  Recomendaciones       => _recomendaciones       ??= new EfRepository<RecomendacionHorario>(_context);
    public IRepository<RegistroAuditoria>    Auditorias           => _auditorias           ??= new EfRepository<RegistroAuditoria>(_context);
    public IRepository<ConfiguracionCorreo>  ConfiguracionCorreo  => _configuracionCorreo  ??= new EfRepository<ConfiguracionCorreo>(_context);
    public IRepository<CorreoEncolado>       CorreosEncolados     => _correosEncolados     ??= new EfRepository<CorreoEncolado>(_context);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
