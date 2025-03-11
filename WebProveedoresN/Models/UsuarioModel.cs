using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        [Required]
        public string? Empresa { get; set; }
        [Required]
        [DisplayName("Nombre Completo")]
        public string? Nombre { get; set; }
        [Required]
        public string? Correo { get; set; }
        [Required]
        [DisplayName("Contraseña")]
        public string? Clave { get; set; }
        public bool Restablecer { get; set; }
        public bool Confirmado { get; set; }
        public string? Token { get; set; }
        public int? IdAcceso { get; set; }
        public int? IdStatus { get; set; }

    }
}
