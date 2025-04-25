using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DataType(DataType.EmailAddress)]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = null!;

    }
}
