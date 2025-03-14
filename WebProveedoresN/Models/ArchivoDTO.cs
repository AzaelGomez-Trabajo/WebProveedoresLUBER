using System;
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
        [DisplayName("Nombre Archivo")]
        public string Nombre { get; set; }
        [Required]
        [DisplayName("Ruta")]
        public string Ruta { get; set; }
        [Required]
        [DisplayName("Fecha y Hora")]
        public string FechaHora { get; set; }
    }
}