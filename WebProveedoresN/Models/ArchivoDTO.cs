using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class ArchivoDTO
    {
        [Required]
        [DisplayName("#")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Nombre")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Ruta")]
        public string Route { get; set; }

        [Required]
        [DisplayName("Fecha y Hora")]
        public string DateTime { get; set; }

        [Required]
        [DisplayName("Numero de Órden")]
        public int OrderId { get; set; }

        [Required]
        [DisplayName("Extención")]
        public string Extension { get; set; }

        [Required]
        [DisplayName("Convertido")]
        public bool Converted { get; set; }
    }
}