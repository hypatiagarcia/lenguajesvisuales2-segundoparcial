using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientesAPI.Models
{
    public class Cliente
    {
        [Key]
        [Required(ErrorMessage = "El CI es obligatorio")]
        [StringLength(20)]
        public string CI { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [StringLength(200)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(500)]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(50)]
        public string Telefono { get; set; } = string.Empty;

        // Almacenamiento de fotografías como datos binarios
        public byte[]? FotoCasa1 { get; set; }
        public byte[]? FotoCasa2 { get; set; }
        public byte[]? FotoCasa3 { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relación con archivos
        public virtual ICollection<ArchivoCliente> Archivos { get; set; } = new List<ArchivoCliente>();
    }
}
