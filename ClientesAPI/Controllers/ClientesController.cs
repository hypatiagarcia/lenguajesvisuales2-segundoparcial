using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientesAPI.Data;
using ClientesAPI.Models;
using ClientesAPI.DTOs;
using ClientesAPI.Services;

namespace ClientesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<ClientesController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo cliente con sus fotografías
        /// </summary>
        /// <param name="clienteDto">Datos del cliente incluyendo fotografías</param>
        /// <returns>Cliente registrado</returns>
        [HttpPost("registrar")]
        [ProducesResponseType(typeof(ApiResponse<ClienteResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<ClienteResponseDto>>> RegistrarCliente([FromForm] ClienteCreateDto clienteDto)
        {
            try
            {
                // Validar si el cliente ya existe
                if (await _context.Clientes.AnyAsync(c => c.CI == clienteDto.CI))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Ya existe un cliente con este CI",
                        Errors = new List<string> { $"El CI {clienteDto.CI} ya está registrado" }
                    });
                }

                // Convertir fotografías a byte arrays
                var cliente = new Cliente
                {
                    CI = clienteDto.CI,
                    Nombres = clienteDto.Nombres,
                    Direccion = clienteDto.Direccion,
                    Telefono = clienteDto.Telefono,
                    FotoCasa1 = await _fileService.ConvertToByteArrayAsync(clienteDto.FotoCasa1),
                    FotoCasa2 = await _fileService.ConvertToByteArrayAsync(clienteDto.FotoCasa2),
                    FotoCasa3 = await _fileService.ConvertToByteArrayAsync(clienteDto.FotoCasa3),
                    FechaRegistro = DateTime.Now
                };

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cliente registrado exitosamente: {cliente.CI} - {cliente.Nombres}");

                var response = new ClienteResponseDto
                {
                    CI = cliente.CI,
                    Nombres = cliente.Nombres,
                    Direccion = cliente.Direccion,
                    Telefono = cliente.Telefono,
                    FechaRegistro = cliente.FechaRegistro,
                    TieneFotoCasa1 = cliente.FotoCasa1 != null,
                    TieneFotoCasa2 = cliente.FotoCasa2 != null,
                    TieneFotoCasa3 = cliente.FotoCasa3 != null
                };

                return CreatedAtAction(nameof(ObtenerCliente), new { ci = cliente.CI },
                    new ApiResponse<ClienteResponseDto>
                    {
                        Success = true,
                        Message = "Cliente registrado exitosamente",
                        Data = response
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cliente");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al registrar el cliente",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene la información de un cliente por su CI
        /// </summary>
        /// <param name="ci">Cédula de identidad del cliente</param>
        /// <returns>Información del cliente</returns>
        [HttpGet("{ci}")]
        [ProducesResponseType(typeof(ApiResponse<ClienteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ClienteResponseDto>>> ObtenerCliente(string ci)
        {
            try
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.CI == ci);

                if (cliente == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    });
                }

                var response = new ClienteResponseDto
                {
                    CI = cliente.CI,
                    Nombres = cliente.Nombres,
                    Direccion = cliente.Direccion,
                    Telefono = cliente.Telefono,
                    FechaRegistro = cliente.FechaRegistro,
                    TieneFotoCasa1 = cliente.FotoCasa1 != null,
                    TieneFotoCasa2 = cliente.FotoCasa2 != null,
                    TieneFotoCasa3 = cliente.FotoCasa3 != null
                };

                return Ok(new ApiResponse<ClienteResponseDto>
                {
                    Success = true,
                    Message = "Cliente encontrado",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cliente con CI: {ci}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener el cliente",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene la lista de todos los clientes
        /// </summary>
        /// <returns>Lista de clientes</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ClienteResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ClienteResponseDto>>>> ObtenerClientes()
        {
            try
            {
                var clientes = await _context.Clientes
                    .Select(c => new ClienteResponseDto
                    {
                        CI = c.CI,
                        Nombres = c.Nombres,
                        Direccion = c.Direccion,
                        Telefono = c.Telefono,
                        FechaRegistro = c.FechaRegistro,
                        TieneFotoCasa1 = c.FotoCasa1 != null,
                        TieneFotoCasa2 = c.FotoCasa2 != null,
                        TieneFotoCasa3 = c.FotoCasa3 != null
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ClienteResponseDto>>
                {
                    Success = true,
                    Message = $"Se encontraron {clientes.Count} clientes",
                    Data = clientes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de clientes");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al obtener la lista de clientes",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtiene una fotografía específica de un cliente
        /// </summary>
        /// <param name="ci">CI del cliente</param>
        /// <param name="numeroFoto">Número de foto (1, 2 o 3)</param>
        /// <returns>Archivo de imagen</returns>
        [HttpGet("{ci}/foto/{numeroFoto}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerFoto(string ci, int numeroFoto)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(ci);
                if (cliente == null)
                {
                    return NotFound(new { message = "Cliente no encontrado" });
                }

                byte[]? foto = numeroFoto switch
                {
                    1 => cliente.FotoCasa1,
                    2 => cliente.FotoCasa2,
                    3 => cliente.FotoCasa3,
                    _ => null
                };

                if (foto == null)
                {
                    return NotFound(new { message = "Fotografía no encontrada" });
                }

                return File(foto, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener foto {numeroFoto} del cliente {ci}");
                return StatusCode(500, new { message = "Error al obtener la fotografía" });
            }
        }
    }
}
