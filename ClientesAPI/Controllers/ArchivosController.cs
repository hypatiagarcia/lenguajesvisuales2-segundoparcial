using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientesAPI.Data;
using ClientesAPI.DTOs;
using ClientesAPI.Services;

namespace ClientesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ArchivosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<ArchivosController> _logger;

        public ArchivosController(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<ArchivosController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Sube múltiples archivos comprimidos en ZIP para un cliente
        /// </summary>
        /// <param name="uploadDto">CI del cliente y archivo ZIP</param>
        /// <returns>Lista de archivos procesados</returns>
        [HttpPost("subir")]
        [ProducesResponseType(typeof(ApiResponse<List<ArchivoResponseDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<List<ArchivoResponseDto>>>> SubirArchivos([FromForm] ArchivoUploadDto uploadDto)
        {
            try
            {
                // Validar que el cliente existe
                var clienteExists = await _context.Clientes.AnyAsync(c => c.CI == uploadDto.CICliente);
                if (!clienteExists)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cliente no encontrado",
                        Errors = new List<string> { $"No existe un cliente con CI: {uploadDto.CICliente}" }
                    });
                }

                // Validar que el archivo es ZIP
                if (uploadDto.ArchivoZip.ContentType != "application/zip" && 
                    uploadDto.ArchivoZip.ContentType != "application/x-zip-compressed" &&
                    !uploadDto.ArchivoZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El archivo debe ser un ZIP",
                        Errors = new List<string> { "Solo se permiten archivos con extensión .zip" }
                    });
                }

                // Procesar el archivo ZIP
                var archivos = await _fileService.ProcessZipFileAsync(uploadDto.CICliente, uploadDto.ArchivoZip);

                if (archivos.Count == 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El archivo ZIP no contiene archivos válidos"
                    });
                }

                // Guardar registros en la base de datos
                _context.ArchivosCliente.AddRange(archivos);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Se procesaron {archivos.Count} archivos para el cliente {uploadDto.CICliente}");

                var response = archivos.Select(a => new ArchivoResponseDto
                {
                    IdArchivo = a.IdArchivo,
                    CICliente = a.CICliente,
                    NombreArchivo = a.NombreArchivo,
                    UrlArchivo = a.UrlArchivo,
                    Extension = a.Extension,
                    TamanoBytes = a.TamanoBytes,
                    FechaSubida = a.FechaSubida
                }).ToList();

                return CreatedAtAction(nameof(ObtenerArchivosPorCliente), new { ciCliente = uploadDto.CICliente },
                    new ApiResponse<List<ArchivoResponseDto>>
                    {
                        Success = true,
                        Message = $"Se subieron {archivos.Count} archivos exitosamente",
                        Data = response
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al subir archivos para cliente {uploadDto.CICliente}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al procesar los archivos",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene todos los archivos de un cliente específico
        /// </summary>
        /// <param name="ciCliente">CI del cliente</param>
        /// <returns>Lista de archivos del cliente</returns>
        [HttpGet("cliente/{ciCliente}")]
        [ProducesResponseType(typeof(ApiResponse<List<ArchivoResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ArchivoResponseDto>>>> ObtenerArchivosPorCliente(string ciCliente)
        {
            try
            {
                var clienteExists = await _context.Clientes.AnyAsync(c => c.CI == ciCliente);
                if (!clienteExists)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    });
                }

                var archivos = await _context.ArchivosCliente
                    .Where(a => a.CICliente == ciCliente)
                    .Select(a => new ArchivoResponseDto
                    {
                        IdArchivo = a.IdArchivo,
                        CICliente = a.CICliente,
                        NombreArchivo = a.NombreArchivo,
                        UrlArchivo = a.UrlArchivo,
                        Extension = a.Extension,
                        TamanoBytes = a.TamanoBytes,
                        FechaSubida = a.FechaSubida
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ArchivoResponseDto>>
                {
                    Success = true,
                    Message = $"Se encontraron {archivos.Count} archivos",
                    Data = archivos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener archivos del cliente {ciCliente}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener los archivos",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene información de un archivo específico
        /// </summary>
        /// <param name="idArchivo">ID del archivo</param>
        /// <returns>Información del archivo</returns>
        [HttpGet("{idArchivo}")]
        [ProducesResponseType(typeof(ApiResponse<ArchivoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ArchivoResponseDto>>> ObtenerArchivo(int idArchivo)
        {
            try
            {
                var archivo = await _context.ArchivosCliente
                    .FirstOrDefaultAsync(a => a.IdArchivo == idArchivo);

                if (archivo == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Archivo no encontrado"
                    });
                }

                var response = new ArchivoResponseDto
                {
                    IdArchivo = archivo.IdArchivo,
                    CICliente = archivo.CICliente,
                    NombreArchivo = archivo.NombreArchivo,
                    UrlArchivo = archivo.UrlArchivo,
                    Extension = archivo.Extension,
                    TamanoBytes = archivo.TamanoBytes,
                    FechaSubida = archivo.FechaSubida
                };

                return Ok(new ApiResponse<ArchivoResponseDto>
                {
                    Success = true,
                    Message = "Archivo encontrado",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener archivo {idArchivo}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener el archivo",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene todos los archivos registrados
        /// </summary>
        /// <returns>Lista de todos los archivos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ArchivoResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ArchivoResponseDto>>>> ObtenerTodosLosArchivos()
        {
            try
            {
                var archivos = await _context.ArchivosCliente
                    .Select(a => new ArchivoResponseDto
                    {
                        IdArchivo = a.IdArchivo,
                        CICliente = a.CICliente,
                        NombreArchivo = a.NombreArchivo,
                        UrlArchivo = a.UrlArchivo,
                        Extension = a.Extension,
                        TamanoBytes = a.TamanoBytes,
                        FechaSubida = a.FechaSubida
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ArchivoResponseDto>>
                {
                    Success = true,
                    Message = $"Se encontraron {archivos.Count} archivos",
                    Data = archivos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los archivos");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener los archivos",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
