namespace Mantenimiento.API.Swagger;

public static class SwaggerMetadata
{
    public static string GetTagName(string controllerName) => controllerName switch
    {
        "Auditoria" => "Auditoría operativa",
        "Auth" => "Autenticación",
        "Configuracion" => "Configuración del sistema",
        "CorreoQueue" => "Cola de correos",
        "Dependencia" => "Gestión de dependencias",
        "Encuesta" => "Encuestas de satisfacción",
        "Horario" => "Importación de horarios",
        "Material" => "Gestión de materiales",
        "OrdenTrabajo" => "Órdenes de trabajo",
        "Programador" => "Gestión del programador",
        "Tecnico" => "Gestión de técnicos",
        _ => controllerName
    };

    public static string GetSchemaName(Type type)
    {
        if (type == typeof(object))
        {
            return "Generico";
        }

        if (type.IsGenericType)
        {
            var genericName = type.Name[..type.Name.IndexOf('`')];
            var genericArguments = type.GetGenericArguments().Select(GetSchemaName).ToArray();
            var genericArgs = string.Join(string.Empty, genericArguments);

            if (genericName == "List")
            {
                return $"ListaDe{genericArgs}";
            }

            if (genericName == "ApiResponse")
            {
                return $"{genericArgs}RespuestaApi";
            }

            return $"{genericArgs}{genericName}";
        }

        return type.Name switch
        {
            "MaterialPreviewRow" => "FilaVistaPreviaMaterial",
            _ => TranslateSchemaName(type.Name)
        };
    }

    private static string TranslateSchemaName(string name)
    {
        if (name == "LoginRequest") return "SolicitudInicioSesion";
        if (name == "LoginResponse") return "RespuestaInicioSesion";
        if (name == "ObjectApiResponse") return "RespuestaApiGenerica";
        if (name == "ActualizarEstadoOrdenRequest") return "SolicitudCambioEstadoOrden";
        if (name == "ActualizarEstadoOperacionRequest") return "SolicitudCambioEstadoOperacion";
        if (name == "AgregarOperacionRequest") return "SolicitudAgregarOperacion";
        if (name == "EditarOperacionRequest") return "SolicitudEditarOperacion";

        if (name.EndsWith("Request", StringComparison.Ordinal))
        {
            var baseName = name[..^"Request".Length];
            return $"Solicitud{baseName}";
        }

        if (name.EndsWith("PagedResult", StringComparison.Ordinal))
        {
            var baseName = name[..^"PagedResult".Length];
            return $"{TranslateSchemaName(baseName)}ResultadoPaginado";
        }

        if (name.EndsWith("Dto", StringComparison.Ordinal))
        {
            return name[..^"Dto".Length];
        }

        return name;
    }

    public static (string Summary, string Description) GetOperationInfo(string controllerName, string actionName) =>
        (controllerName, actionName) switch
        {
            ("Auth", "Login") => (
                "Iniciar sesión y obtener token JWT",
                "Valida las credenciales del usuario y devuelve el token requerido para consumir los procesos protegidos."
            ),

            ("Auditoria", "GetAll") => (
                "Consultar registros de auditoría",
                "Obtiene el historial de auditoría con filtros por fecha, tipo de entidad y usuario."
            ),
            ("Auditoria", "GetById") => (
                "Consultar detalle de auditoría",
                "Devuelve un registro específico del historial de auditoría."
            ),

            ("Configuracion", "Get") => (
                "Consultar configuración de correo",
                "Obtiene la configuración actual usada para el envío de correos del sistema."
            ),
            ("Configuracion", "Update") => (
                "Actualizar configuración de correo",
                "Guarda los parámetros de configuración para el envío de correos."
            ),
            ("Configuracion", "EnviarPrueba") => (
                "Enviar correo de prueba",
                "Valida la configuración actual enviando un correo de prueba a la dirección indicada."
            ),

            ("CorreoQueue", "GetAll") => (
                "Consultar cola de correos",
                "Lista los correos encolados con filtros operativos y de seguimiento."
            ),
            ("CorreoQueue", "GetStats") => (
                "Consultar estadísticas de la cola",
                "Resume la cantidad de correos agrupados por estado dentro de la cola de envío."
            ),
            ("CorreoQueue", "GetById") => (
                "Consultar detalle de un correo encolado",
                "Devuelve la información completa de un correo registrado en la cola."
            ),
            ("CorreoQueue", "Reintentar") => (
                "Reencolar un correo fallido",
                "Vuelve a poner en cola un correo específico para intentar su envío nuevamente."
            ),
            ("CorreoQueue", "ReintentarFallidos") => (
                "Reencolar correos fallidos",
                "Reencola de forma masiva todos los correos que quedaron en estado fallido."
            ),

            ("Dependencia", "GetAll") => (
                "Consultar dependencias registradas",
                "Obtiene el catálogo paginado de dependencias disponibles para asociar usuarios y órdenes."
            ),
            ("Dependencia", "GetById") => (
                "Consultar detalle de una dependencia",
                "Devuelve la información de una dependencia específica por su identificador."
            ),
            ("Dependencia", "Create") => (
                "Registrar una nueva dependencia",
                "Crea una dependencia para su uso en la asignación y clasificación operativa."
            ),
            ("Dependencia", "Update") => (
                "Actualizar una dependencia existente",
                "Modifica los datos de una dependencia ya registrada."
            ),
            ("Dependencia", "Delete") => (
                "Eliminar una dependencia",
                "Retira una dependencia del catálogo administrativo."
            ),

            ("Encuesta", "Registrar") => (
                "Registrar encuesta de satisfacción",
                "Guarda la evaluación del solicitante sobre la atención recibida en el proceso de mantenimiento."
            ),
            ("Encuesta", "GetResultados") => (
                "Consultar resultados de encuestas",
                "Recupera las encuestas registradas, con filtros de fecha para análisis administrativo."
            ),
            ("Encuesta", "GetMetricas") => (
                "Consultar métricas de satisfacción",
                "Resume indicadores de satisfacción por técnico y por dependencia."
            ),

            ("Horario", "Importar") => (
                "Importar horarios de técnicos",
                "Carga horarios en lote identificando a cada técnico por su correo electrónico."
            ),

            ("Material", "GetAll") => (
                "Consultar inventario de materiales",
                "Obtiene el listado paginado de materiales disponibles para mantenimiento."
            ),
            ("Material", "GetById") => (
                "Consultar detalle de un material",
                "Devuelve la información completa de un material específico."
            ),
            ("Material", "Create") => (
                "Registrar material en inventario",
                "Crea un nuevo material para que pueda ser usado en solicitudes y operaciones."
            ),
            ("Material", "Update") => (
                "Actualizar material de inventario",
                "Modifica la información base de un material existente."
            ),
            ("Material", "Delete") => (
                "Eliminar material del inventario",
                "Elimina un material siempre que no esté siendo utilizado en otros procesos."
            ),
            ("Material", "PreviewExcel") => (
                "Validar archivo Excel de materiales",
                "Procesa un archivo Excel y devuelve una vista previa con filas válidas y errores detectados antes de la carga masiva."
            ),
            ("Material", "BulkCreate") => (
                "Registrar materiales de forma masiva",
                "Crea múltiples materiales en un solo proceso de carga."
            ),

            ("OrdenTrabajo", "GetAll") => (
                "Consultar órdenes de trabajo",
                "Lista las órdenes de trabajo usando filtros operativos como estado, urgencia, dependencia o técnico."
            ),
            ("OrdenTrabajo", "CrearOrden") => (
                "Crear orden de trabajo",
                "Registra una nueva solicitud de mantenimiento dentro del flujo operativo."
            ),
            ("OrdenTrabajo", "GetOrdenesPorTecnico") => (
                "Consultar órdenes asignadas a un técnico",
                "Muestra las órdenes relacionadas con un técnico específico para seguimiento de carga operativa."
            ),
            ("OrdenTrabajo", "GetById") => (
                "Consultar detalle de una orden de trabajo",
                "Devuelve la orden con su trazabilidad, asignaciones y operaciones asociadas."
            ),
            ("OrdenTrabajo", "ActualizarEstado") => (
                "Cambiar estado de una orden de trabajo",
                "Actualiza el estado general de la orden dentro del proceso de ejecución."
            ),
            ("OrdenTrabajo", "AgregarOperacion") => (
                "Agregar operación a una orden",
                "Registra una nueva operación técnica dentro de una orden de trabajo."
            ),
            ("OrdenTrabajo", "ActualizarEstadoOperacion") => (
                "Cambiar estado de una operación",
                "Permite al técnico avanzar o pausar una operación específica."
            ),
            ("OrdenTrabajo", "EliminarOperacion") => (
                "Eliminar operación de una orden",
                "Retira una operación registrada de la orden de trabajo."
            ),
            ("OrdenTrabajo", "EditarOperacion") => (
                "Editar operación de una orden",
                "Actualiza número, descripción y horas hombre de una operación existente."
            ),
            ("OrdenTrabajo", "RegistrarActividad") => (
                "Registrar actividad de ejecución",
                "Guarda pausas, finalizaciones y consumo operativo asociado a una operación."
            ),

            ("Programador", "GetTecnicos") => (
                "Consultar técnicos a cargo",
                "Obtiene los técnicos asociados al programador autenticado."
            ),
            ("Programador", "GetOrdenes") => (
                "Consultar órdenes del equipo asignado",
                "Lista las órdenes relacionadas con los técnicos a cargo del programador."
            ),
            ("Programador", "ExportOrdenesExcel") => (
                "Exportar órdenes a Excel",
                "Genera un archivo Excel con el resumen de órdenes y sus operaciones."
            ),
            ("Programador", "GetNotificaciones") => (
                "Consultar notificaciones",
                "Obtiene las notificaciones del usuario autenticado, con opción de filtrar solo no leídas."
            ),
            ("Programador", "MarcarLeida") => (
                "Marcar notificación como leída",
                "Actualiza una notificación específica para dejarla en estado leída."
            ),
            ("Programador", "MarcarTodasLeidas") => (
                "Marcar todas las notificaciones como leídas",
                "Actualiza todas las notificaciones del usuario autenticado como leídas."
            ),
            ("Programador", "AprobarMateriales") => (
                "Aprobar materiales solicitados",
                "Aprueba materiales pendientes y notifica al técnico involucrado."
            ),
            ("Programador", "GetTurnos") => (
                "Consultar turnos de un técnico",
                "Obtiene los turnos de un técnico dentro de un rango de fechas."
            ),
            ("Programador", "SugerirHorario") => (
                "Sugerir horario de trabajo",
                "Genera una propuesta de turnos con base en las reglas definidas por el proceso."
            ),
            ("Programador", "GuardarTurnos") => (
                "Guardar turnos programados",
                "Registra los turnos definidos para los técnicos."
            ),
            ("Programador", "EliminarTurno") => (
                "Eliminar turno programado",
                "Retira un turno específico del calendario operativo."
            ),
            ("Programador", "GetConfiguracion") => (
                "Consultar configuración operativa",
                "Obtiene la configuración general usada en la programación del servicio."
            ),
            ("Programador", "ActualizarConfiguracion") => (
                "Actualizar configuración operativa",
                "Guarda cambios en la configuración general usada por la programación."
            ),

            ("Tecnico", "GetAll") => (
                "Consultar técnicos registrados",
                "Obtiene el listado de técnicos disponibles para asignación y administración."
            ),
            ("Tecnico", "GetById") => (
                "Consultar detalle de un técnico",
                "Devuelve la información de un técnico específico."
            ),
            ("Tecnico", "Create") => (
                "Registrar técnico",
                "Crea un nuevo usuario técnico dentro del sistema."
            ),
            ("Tecnico", "Update") => (
                "Actualizar técnico",
                "Modifica los datos administrativos de un técnico existente."
            ),
            ("Tecnico", "Delete") => (
                "Eliminar técnico",
                "Retira un técnico del catálogo administrativo."
            ),

            _ => (actionName, $"Operación expuesta por el controlador {controllerName}.")
        };

    public static string GetResponseDescription(string statusCode) => statusCode switch
    {
        "200" => "Operación completada correctamente.",
        "201" => "Recurso creado correctamente.",
        "400" => "La solicitud contiene datos inválidos o no cumple las reglas del proceso.",
        "401" => "La solicitud requiere autenticación válida.",
        "403" => "El usuario autenticado no tiene permisos para ejecutar esta acción.",
        "404" => "No se encontró el recurso solicitado.",
        "405" => "El método HTTP no está permitido para este recurso.",
        "500" => "Ocurrió un error interno en el servidor.",
        _ => $"Respuesta HTTP {statusCode}."
    };
}
