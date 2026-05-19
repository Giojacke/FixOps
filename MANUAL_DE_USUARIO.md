# Manual de Usuario

## 1. Propósito del sistema

FixOps es una aplicación para gestionar órdenes de trabajo de mantenimiento, controlar su ejecución, registrar encuestas de satisfacción, administrar catálogos y dar seguimiento a notificaciones por correo.

El sistema tiene dos componentes visibles para el usuario:

- Aplicación web: interfaz principal para operar el sistema.
- API con Swagger: interfaz técnica para pruebas e integración.

## 2. Acceso al sistema

### 2.1. URL de acceso

En ambiente local del proyecto:

- Web: `http://localhost:5166`
- API Swagger: `http://localhost:5284/swagger/index.html`

### 2.2. Inicio de sesión

Pantalla: `/login`

Debe ingresar:

- Correo electrónico
- Contraseña

En la pantalla actual del sistema existe un usuario de demostración:

- Usuario: `admin@mantenimiento.com`
- Contraseña: `Admin123!`

Después de iniciar sesión, el sistema redirige a la pantalla principal de órdenes.

### 2.3. Cierre de sesión

Desde el menú lateral, use la opción `Cerrar Sesión`.

## 3. Roles del sistema

El menú y las acciones visibles cambian según el rol del usuario.

### 3.1. Administrador

Puede acceder a:

- Órdenes
- Nueva Orden
- Maestros
- Analítica
- Configuración

Además, puede:

- Cancelar órdenes activas
- Gestionar materiales, dependencias y usuarios
- Consultar resultados de encuestas
- Ver métricas operativas
- Configurar correo SMTP y revisar la cola de correos
- Configurar parámetros de horarios

### 3.2. Solicitante

Puede acceder a:

- Órdenes
- Nueva Orden

Además, puede:

- Registrar órdenes
- Consultar el estado de sus órdenes
- Responder encuestas cuando una orden finaliza

### 3.3. Técnico

Puede acceder a:

- Órdenes asignadas
- Horario de Trabajo

Además, puede:

- Desarrollar operaciones asignadas
- Registrar pausas
- Solicitar materiales durante una pausa
- Finalizar operaciones
- Consultar su horario semanal

### 3.4. Programador

Puede acceder a:

- Órdenes
- Mi Panel

Además, puede:

- Supervisar órdenes de sus técnicos
- Aprobar materiales solicitados en operaciones pausadas
- Gestionar horarios de técnicos
- Exportar órdenes a Excel
- Administrar materiales desde su panel

## 4. Menú principal

El menú lateral puede mostrar las siguientes opciones según permisos:

- `Órdenes`
- `Nueva Orden`
- `Maestros`
- `Analítica`
- `Configuración`
- `Mi Panel`
- `Horario de Trabajo`
- `Cerrar Sesión`

## 5. Gestión de órdenes

### 5.1. Consulta de órdenes

Pantalla principal: `/`

Aquí se muestra el listado de órdenes de trabajo.

Información visible en la tabla:

- Folio
- Descripción
- Dependencia
- Técnico asignado
- Estado
- Urgencia
- Fecha

Los usuarios con rol Administrador y Solicitante disponen de filtros por:

- Estado
- Urgencia
- Folio
- Fecha desde
- Fecha hasta

El técnico visualiza únicamente sus órdenes asignadas.

### 5.2. Estados generales de una orden

El sistema maneja estos estados visibles:

- `Pendiente`
- `EnProceso`
- `Finalizada`
- `Cancelada`

### 5.3. Crear una nueva orden

Pantalla: `/registrar-orden`

Disponible para:

- Administrador
- Solicitante

Datos solicitados:

- Descripción del problema
- Dependencia
- Contacto solicitante
- Técnico asignado
- Urgencia
- Primera operación

La primera operación se registra junto con la orden. Debe indicar:

- Número de operación
- Función a realizar

Al guardar correctamente, el sistema regresa al listado principal.

### 5.4. Cancelar una orden

Disponible para:

- Administrador

Solo puede cancelarse una orden activa. El sistema solicita confirmación antes de ejecutar la acción.

## 6. Gestión de operaciones

### 6.1. Ver y administrar operaciones

Pantalla: `/añadir-operacion/{ordenId}`

Disponible para:

- Administrador
- Técnico
- Programador

Desde esta pantalla se puede:

- Ver el detalle de la orden
- Consultar operaciones registradas
- Agregar nuevas operaciones
- Editar operaciones
- Eliminar operaciones
- Abrir el formulario de desarrollo de actividad

Notas importantes:

- El Técnico no administra la estructura de las operaciones; trabaja sobre las asignadas.
- Administrador y Programador pueden agregar, editar o eliminar operaciones mientras la orden no esté finalizada o cancelada.
- Si la operación ya está finalizada, no puede modificarse.

### 6.2. Numeración de operaciones

El sistema usa una secuencia de números de operación predefinida. Si todos los números disponibles ya fueron usados, no será posible crear una nueva operación adicional.

## 7. Registro de actividad del técnico

### 7.1. Abrir una operación

Pantalla: `/actividad/{ordenId}/{operacionId}`

Disponible para:

- Técnico

### 7.2. Tipos de registro

El técnico puede registrar:

- `Pausa`
- `Finalización`

### 7.3. Registrar una pausa

Debe indicar:

- Fecha y hora de inicio
- Motivo de pausa

Motivos visibles en el sistema:

- Necesidad de materiales
- Fin de turno
- Fuera de alcance
- Equipo no disponible
- Atender otro servicio

Si el motivo es `Necesidad de materiales`, debe agregar al menos un material solicitado.

Puede registrar:

- Material del catálogo
- Material diferente o no catalogado

Cada material requiere:

- Nombre o selección del material
- Cantidad

### 7.4. Finalizar una operación

Debe indicar:

- Fecha y hora de inicio
- Fecha y hora de fin
- Detalle de finalización

El sistema:

- Valida que la fecha final sea posterior al inicio
- Calcula la duración
- Solicita confirmación antes de finalizar

Una operación finalizada ya no puede modificarse.

## 8. Aprobación de materiales

### 8.1. Flujo funcional

Cuando un técnico pausa una operación por necesidad de materiales:

1. La operación queda en pausa.
2. El Programador puede revisar la solicitud.
3. El Programador aprueba los materiales y notifica al técnico.

### 8.2. Dónde se aprueba

Puede hacerse desde:

- El panel del Programador
- El detalle de operaciones de una orden

El Programador puede agregar un mensaje adicional antes de aprobar.

## 9. Encuesta de satisfacción

### 9.1. Cuándo aparece

La encuesta se habilita cuando la orden ya está finalizada y aún no tiene encuesta registrada.

### 9.2. Pantalla

Ruta: `/encuesta/{ordenId}`

### 9.3. Información que muestra

Antes de responder, el usuario ve:

- Folio
- Dependencia
- Descripción original
- Operaciones ejecutadas

### 9.4. Campos de la encuesta

La encuesta permite calificar de 1 a 5:

- Atención
- Servicio
- Tiempo

También incluye:

- Comentarios opcionales

Al enviar la encuesta, el sistema registra la respuesta y regresa a la pantalla principal.

## 10. Módulo Maestros

Pantalla: `/maestros`

Disponible para:

- Administrador

Este módulo tiene tres pestañas:

- Dependencias
- Materiales
- Roles

### 10.1. Dependencias

Permite:

- Crear dependencia
- Editar dependencia
- Eliminar dependencia
- Descargar plantilla Excel
- Cargar dependencias de forma masiva por Excel

Datos principales:

- Nombre
- Código
- Regional
- Departamento
- Ubicación
- Contacto
- Correo de la dependencia

### 10.2. Materiales

Permite:

- Crear material
- Editar material
- Eliminar material
- Carga masiva por Excel

Datos principales:

- Nombre
- Tipo
- Descripción
- Stock
- Precio unitario

Si un material está en uso, el sistema puede impedir su eliminación.

### 10.3. Roles y usuarios

Permite gestionar usuarios por rol:

- Técnicos
- Programadores
- Administradores

Acciones disponibles:

- Crear usuario
- Editar usuario
- Eliminar usuario
- Cargar usuarios por Excel
- Descargar plantilla
- Asignar programador a un técnico

Observación:

- Los usuarios creados desde este módulo reciben contraseña temporal `Admin123!`.

## 11. Panel del Programador

Pantalla: `/programador`

Disponible para:

- Programador

El panel tiene tres pestañas:

- Órdenes
- Horarios
- Materiales

### 11.1. Pestaña Órdenes

Muestra órdenes de los técnicos bajo supervisión del programador.

Permite:

- Expandir una orden para ver sus operaciones
- Consultar estado de cada operación
- Ver solicitudes de materiales
- Aprobar materiales en operaciones pausadas
- Ir a la gestión detallada de operaciones
- Exportar órdenes a Excel

### 11.2. Pestaña Horarios

Muestra la lista de técnicos asignados al programador.

Permite abrir la gestión detallada de horario por técnico.

### 11.3. Pestaña Materiales

Permite al Programador:

- Crear materiales
- Importar materiales desde Excel
- Exportar materiales a Excel
- Descargar plantilla de importación

## 12. Gestión de horarios

### 12.1. Horario del técnico por programador

Pantalla: `/programador/horario/{tecnicoId}`

Disponible para:

- Programador

Funciones:

- Ver horario por período semanal, quincenal o mensual
- Elegir fecha de inicio
- Crear o editar turnos
- Eliminar turnos
- Guardar cambios
- Descartar cambios
- Solicitar sugerencia automática de horario

Cada turno permite definir:

- Hora de inicio
- Hora de fin
- Si el almuerzo va incluido en la jornada

La vista muestra acumulados semanales de horas.

### 12.2. Vista de horario del técnico

Pantalla: `/mi-horario`

Disponible para:

- Técnico

Funciones:

- Ver su semana actual
- Cambiar a semana anterior o siguiente
- Ir a la semana actual
- Consultar horas efectivas
- Ver días asignados, días libres y jornadas con almuerzo

Si no existen turnos cargados para la semana, el sistema muestra una advertencia para contactar al programador.

## 13. Analítica

Disponible para:

- Administrador

El menú `Analítica` incluye:

- `Resultados Encuestas`
- `Métricas`

### 13.1. Resultados de encuestas

Pantalla: `/encuestas/resultados`

Permite:

- Filtrar por fechas
- Ver promedio por técnico
- Ver promedio por dependencia
- Consultar el detalle de encuestas registradas
- Ver promedio general del período consultado

### 13.2. Métricas operativas

Pantalla: `/metricas`

Permite:

- Filtrar por regional
- Consultar por período
- Usar rango de fechas personalizado

Indicadores visibles:

- Total de órdenes
- Tiempo promedio de resolución
- Órdenes finalizadas
- Órdenes pendientes y en proceso

Gráficos visibles:

- Distribución por estado
- Órdenes por regional
- Tiempo promedio por regional
- Tendencia mensual

## 14. Configuración

Pantalla: `/configuracion`

Disponible para:

- Administrador

Incluye tres pestañas:

- Correo SMTP
- Cola de envíos
- Horarios

### 14.1. Correo SMTP

Permite configurar:

- Servidor SMTP
- Puerto
- Usuario
- Contraseña
- Correo remitente
- Nombre remitente
- Activación de SSL/TLS

También permite enviar un correo de prueba a una dirección específica.

### 14.2. Cola de envíos

Permite consultar el historial y estado de correos registrados por el sistema.

Estados visibles:

- Pendiente
- Procesando
- Enviado
- Fallido

Información visible:

- Destinatario
- Asunto
- Tipo de correo
- Estado
- Intentos realizados
- Fecha de creación
- Error, si aplica

Funciones disponibles:

- Filtrar por estado
- Actualizar el listado
- Reintentar un correo individual
- Reintentar todos los correos fallidos

Nota:

- El procesamiento de la cola es automático mediante un servicio en segundo plano.

### 14.3. Parámetros de horarios

Permite definir reglas generales de jornada:

- Horas semanales máximas
- Horas extras máximas
- Horas efectivas diarias
- Hora de inicio predeterminada
- Si el almuerzo se considera dentro de la jornada

También permite administrar recomendaciones de horario:

- Crear recomendación
- Editar recomendación
- Eliminar recomendación
- Activar o desactivar recomendación

## 15. Uso de Swagger

Swagger es la interfaz de consulta de la API.

Ruta:

- `http://localhost:5284/swagger/index.html`

Desde Swagger se puede:

- Ver endpoints disponibles
- Consultar parámetros y modelos
- Probar operaciones HTTP
- Autenticarse con token Bearer si la operación lo requiere

Swagger está orientado a pruebas técnicas. La operación normal del sistema debe hacerse desde la aplicación web.

## 16. Recomendaciones de uso

- Cree primero las dependencias, materiales y usuarios antes de operar en producción.
- Asigne programadores a los técnicos para habilitar el flujo de supervisión.
- Complete la configuración de correo si desea usar notificaciones.
- Use carga masiva por Excel cuando deba registrar grandes volúmenes de catálogos.
- Revise la cola de correos si un técnico o usuario reporta que no recibió una notificación.
- Verifique periódicamente las encuestas y métricas para identificar oportunidades de mejora.

## 17. Solución de problemas frecuentes

### 17.1. No puedo ver una opción del menú

Posible causa:

- Su rol no tiene permisos para esa función.

Acción recomendada:

- Verifique con un Administrador el rol asignado a su usuario.

### 17.2. No puedo finalizar una operación

Posibles causas:

- La fecha y hora de fin son inválidas.
- El detalle de finalización está vacío.
- La operación ya fue finalizada.

### 17.3. No puedo registrar una pausa por materiales

Posible causa:

- No se agregó al menos un material válido o la cantidad es incorrecta.

### 17.4. No recibí un correo

Acciones recomendadas:

- Revisar la configuración SMTP.
- Consultar la pestaña `Cola de envíos`.
- Reintentar el correo fallido si corresponde.

### 17.5. No tengo horario asignado

Posible causa:

- El programador aún no ha registrado turnos para la semana consultada.

## 18. Glosario breve

- `Orden de trabajo`: solicitud de mantenimiento registrada en el sistema.
- `Operación`: actividad específica dentro de una orden.
- `Programador`: responsable de coordinar técnicos, materiales y horarios.
- `Dependencia`: área o unidad solicitante del servicio.
- `Cola de correos`: registro y procesamiento interno de notificaciones por email.

