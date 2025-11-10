using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientesAPI.Models
{
    public class LogApi
    {
        [Key]
        public int IdLog { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime DateTime { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string TipoLog { get; set; } = string.Empty; // INFO, ERROR, WARNING

        public string? RequestBody { get; set; }

        public string? ResponseBody { get; set; }

        [StringLength(500)]
        public string? UrlEndpoint { get; set; }

        [StringLength(10)]
        public string? MetodoHttp { get; set; }

        [StringLength(50)]
        public string? DireccionIp { get; set; }

        public string? Detalle { get; set; }

        public int StatusCode { get; set; }

        public double DuracionMs { get; set; }
    }
}
