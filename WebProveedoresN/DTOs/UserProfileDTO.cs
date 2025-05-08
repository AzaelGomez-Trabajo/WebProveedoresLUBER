using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.DTOs
{
    public class UserProfileDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [StringLength(50, ErrorMessage = "El campo {0} no puede exceder los {1} caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Correo Electrónico")]
        [EmailAddress(ErrorMessage = "El formato del {0} no es válido.")]
        public string Email { get; set; } = null!;

        public string Token { get; set; } = null!;
    }
}
