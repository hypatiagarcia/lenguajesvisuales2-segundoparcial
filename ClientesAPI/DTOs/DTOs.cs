using System.ComponentModel.DataAnnotations;

namespace ClientesAPI.DTOs
{
    public class ClienteCreateDto
    {
        [Required(ErrorMessage = "El CI es obligatorio")]
        public string CI { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = string.Empty;

        public IFormFile? FotoCasa1 { get; set; }
        public IFormFile? FotoCasa2 { get; set; }
        public IFormFile? FotoCasa3 { get; set; }
    }

    public class ClienteResponseDto
    {
        public string CI { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public bool TieneFotoCasa1 { get; set; }
        public bool TieneFotoCasa2 { get; set; }
        public bool TieneFotoCasa3 { get; set; }
    }

    public class ArchivoUploadDto
    {
        [Required(ErrorMessage = "El CI del cliente es obligatorio")]
        public string CICliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El archivo ZIP es obligatorio")]
        public IFormFile ArchivoZip { get; set; } = null!;
    }

    public class ArchivoResponseDto
    {
        public int IdArchivo { get; set; }
        public string CICliente { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string UrlArchivo { get; set; } = string.Empty;
        public string? Extension { get; set; }
        public long TamanoBytes { get; set; }
        public DateTime FechaSubida { get; set; }
    }

    public class LogApiResponseDto
    {
        public int IdLog { get; set; }
        public DateTime DateTime { get; set; }
        public string TipoLog { get; set; } = string.Empty;
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public string? UrlEndpoint { get; set; }
        public string? MetodoHttp { get; set; }
        public string? DireccionIp { get; set; }
        public string? Detalle { get; set; }
        public int StatusCode { get; set; }
        public double DuracionMs { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
    }
}
