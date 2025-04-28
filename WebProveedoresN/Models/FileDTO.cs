using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class FileDTO
    {
        [Required]
        [DisplayName("#")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Nombre")]
        public string Name { get; set; } = null!;

        [Required]
        [DisplayName("Ruta")]
        public string Route { get; set; } = null!;

        [Required]
        [DisplayName("Fecha y Hora")]
        public string DateTime { get; set; } = null!;

        [Required]
        [DisplayName("Numero de Órden")]
        public int OrderNumber { get; set; }

        [Required]
        [DisplayName("Extención")]
        public string Extension { get; set; } = null!;

        [Required]
        [DisplayName("Convertido")]
        public bool Converted { get; set; }
    }
}