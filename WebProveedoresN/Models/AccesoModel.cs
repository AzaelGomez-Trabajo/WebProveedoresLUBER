using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class AccesoModel
    {
        public int IdAcceso { get; set; }
        [Required]
        public string Acceso { get; set; }
    }
}
