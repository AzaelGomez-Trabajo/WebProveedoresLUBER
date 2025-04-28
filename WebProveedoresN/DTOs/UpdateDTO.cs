using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.DTOs
{
    public class UpdateDTO
    {
        public string Token { get; set; } = null!;

        public bool Restablecer { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [StringLength(20, ErrorMessage = "El campo {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 3)]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [StringLength(20, ErrorMessage = "El campo {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 3)]
        public string PasswordConfirm { get; set; } = null!;
    }
}
