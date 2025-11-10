using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientesAPI.Data;
using ClientesAPI.DTOs;

namespace ClientesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LogsController> _logger;

        public LogsController(ApplicationDbContext context, ILogger<LogsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los registros de logs
        /// </summary>
        /// <param name="limite">Número máximo de registros a retornar (default: 100)</param>
        /// <param name="tipoLog">Filtrar por tipo de log (INFO, ERROR, WARNING)</param>
        /// <returns>Lista de logs</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<LogApiResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<LogApiResponseDto>>>> ObtenerLogs(
            [FromQuery] int limite = 100,
            [FromQuery] string? tipoLog = null)
        {
            try
            {
                var query = _context.LogsApi.AsQueryable();

                // Filtrar por tipo si se especifica
                if (!string.IsNullOrEmpty(tipoLog))
                {
                    query = query.Where(l => l.TipoLog.ToUpper() == tipoLog.ToUpper());
                }

                var logs = await query
                    .OrderByDescending(l => l.DateTime)
                    .Take(limite)
                    .Select(l => new LogApiResponseDto
                    {
                        IdLog = l.IdLog,
                        DateTime = l.DateTime,
                        TipoLog = l.TipoLog,
                        RequestBody = l.RequestBody,
                        ResponseBody = l.ResponseBody,
                        UrlEndpoint = l.UrlEndpoint,
                        MetodoHttp = l.MetodoHttp,
                        DireccionIp = l.DireccionIp,
                        Detalle = l.Detalle,
                        StatusCode = l.StatusCode,
                        DuracionMs = l.DuracionMs
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<LogApiResponseDto>>
                {
                    Success = true,
                    Message = $"Se encontraron {logs.Count} registros de logs",
                    Data = logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener los logs",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene un log específico por su ID
        /// </summary>
        /// <param name="idLog">ID del log</param>
        /// <returns>Información del log</returns>
        [HttpGet("{idLog}")]
        [ProducesResponseType(typeof(ApiResponse<LogApiResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<LogApiResponseDto>>> ObtenerLog(int idLog)
        {
            try
            {
                var log = await _context.LogsApi
                    .FirstOrDefaultAsync(l => l.IdLog == idLog);

                if (log == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Log no encontrado"
                    });
                }

                var response = new LogApiResponseDto
                {
                    IdLog = log.IdLog,
                    DateTime = log.DateTime,
                    TipoLog = log.TipoLog,
                    RequestBody = log.RequestBody,
                    ResponseBody = log.ResponseBody,
                    UrlEndpoint = log.UrlEndpoint,
                    MetodoHttp = log.MetodoHttp,
                    DireccionIp = log.DireccionIp,
                    Detalle = log.Detalle,
                    StatusCode = log.StatusCode,
                    DuracionMs = log.DuracionMs
                };

                return Ok(new ApiResponse<LogApiResponseDto>
                {
                    Success = true,
                    Message = "Log encontrado",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener log {idLog}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener el log",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de los logs
        /// </summary>
        /// <returns>Estadísticas de uso de la API</returns>
        [HttpGet("estadisticas")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> ObtenerEstadisticas()
        {
            try
            {
                var totalLogs = await _context.LogsApi.CountAsync();
                var logsInfo = await _context.LogsApi.CountAsync(l => l.TipoLog == "INFO");
                var logsError = await _context.LogsApi.CountAsync(l => l.TipoLog == "ERROR");
                var logsWarning = await _context.LogsApi.CountAsync(l => l.TipoLog == "WARNING");
                var duracionPromedio = await _context.LogsApi.AverageAsync(l => (double?)l.DuracionMs) ?? 0;

                var endpointsMasUsados = await _context.LogsApi
                    .GroupBy(l => new { l.UrlEndpoint, l.MetodoHttp })
                    .Select(g => new
                    {
                        Endpoint = g.Key.UrlEndpoint,
                        Metodo = g.Key.MetodoHttp,
                        Cantidad = g.Count()
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .Take(10)
                    .ToListAsync();

                var estadisticas = new
                {
                    TotalLogs = totalLogs,
                    LogsPorTipo = new
                    {
                        INFO = logsInfo,
                        ERROR = logsError,
                        WARNING = logsWarning
                    },
                    DuracionPromedioMs = Math.Round(duracionPromedio, 2),
                    EndpointsMasUsados = endpointsMasUsados,
                    UltimaActualizacion = DateTime.Now
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Estadísticas obtenidas exitosamente",
                    Data = estadisticas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener estadísticas",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene logs filtrados por fecha
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio (formato: yyyy-MM-dd)</param>
        /// <param name="fechaFin">Fecha fin (formato: yyyy-MM-dd)</param>
        /// <returns>Lista de logs en el rango de fechas</returns>
        [HttpGet("por-fecha")]
        [ProducesResponseType(typeof(ApiResponse<List<LogApiResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<LogApiResponseDto>>>> ObtenerLogsPorFecha(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                var logs = await _context.LogsApi
                    .Where(l => l.DateTime >= fechaInicio && l.DateTime <= fechaFin)
                    .OrderByDescending(l => l.DateTime)
                    .Select(l => new LogApiResponseDto
                    {
                        IdLog = l.IdLog,
                        DateTime = l.DateTime,
                        TipoLog = l.TipoLog,
                        RequestBody = l.RequestBody,
                        ResponseBody = l.ResponseBody,
                        UrlEndpoint = l.UrlEndpoint,
                        MetodoHttp = l.MetodoHttp,
                        DireccionIp = l.DireccionIp,
                        Detalle = l.Detalle,
                        StatusCode = l.StatusCode,
                        DuracionMs = l.DuracionMs
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<LogApiResponseDto>>
                {
                    Success = true,
                    Message = $"Se encontraron {logs.Count} registros entre {fechaInicio:yyyy-MM-dd} y {fechaFin:yyyy-MM-dd}",
                    Data = logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs por fecha");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener logs por fecha",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
