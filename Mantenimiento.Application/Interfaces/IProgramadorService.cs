using Mantenimiento.Application.Common;
using Mantenimiento.Application.DTOs;

namespace Mantenimiento.Application.Interfaces;

public interface IProgramadorService
{
    // Técnicos a cargo
    Task<Result<IEnumerable<UsuarioDto>>> GetTecnicosACargo(Guid programadorId);

    // Órdenes de sus técnicos
    Task<Result<IEnumerable<OrdenTrabajoDto>>> GetOrdenesDeTenicosAsync(Guid programadorId);

    // Notificaciones
    Task<Result<IEnumerable<NotificacionDto>>> GetNotificacionesAsync(Guid usuarioId, bool soloNoLeidas = false);
    Task<Result> MarcarLeidaAsync(Guid notificacionId, Guid usuarioId);
    Task<Result> MarcarTodasLeidasAsync(Guid usuarioId);
    Task<Result> EnviarNotificacionAsync(Guid destinatarioId, string mensaje, Guid? ordenId, Guid? operacionId);

    // Aprobación de materiales
    Task<Result> AprobarMaterialesYNotificarAsync(AprobacionMaterialRequest request, Guid programadorId);

    // Horarios
    Task<Result<IEnumerable<TurnoHorarioDto>>> GetTurnosAsync(Guid tecnicoId, DateOnly desde, DateOnly hasta);
    Task<Result<IEnumerable<TurnoHorarioDto>>> SugerirHorarioAsync(SugerirHorarioRequest request);
    Task<Result> GuardarTurnosAsync(GuardarTurnosRequest request, Guid programadorId);
    Task<Result> EliminarTurnoAsync(Guid turnoId, Guid programadorId);

    // Importación masiva de horarios
    Task<ImportarHorariosResult> ImportarHorariosAsync(ImportarHorariosRequest request);

    // Configuración empresa
    Task<Result<ConfiguracionEmpresaDto>> GetConfiguracionAsync();
    Task<Result> ActualizarConfiguracionAsync(ConfiguracionEmpresaDto dto);

    // Recomendaciones de horario
    Task<Result<IEnumerable<RecomendacionHorarioDto>>> GetRecomendacionesAsync();
    Task<Result<RecomendacionHorarioDto>> CrearRecomendacionAsync(RecomendacionHorarioDto dto);
    Task<Result> ActualizarRecomendacionAsync(RecomendacionHorarioDto dto);
    Task<Result> EliminarRecomendacionAsync(Guid id);
}
