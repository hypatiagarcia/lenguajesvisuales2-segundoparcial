using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientesAPI.Models
{
    public class ArchivoCliente
    {
        [Key]
        public int IdArchivo { get; set; }

        [Required]
        [StringLength(20)]
        [ForeignKey("Cliente")]
        public string CICliente { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string UrlArchivo { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Extension { get; set; }

        public long TamanoBytes { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime FechaSubida { get; set; } = DateTime.Now;

        // Navegación
        public virtual Cliente? Cliente { get; set; }
    }
}
