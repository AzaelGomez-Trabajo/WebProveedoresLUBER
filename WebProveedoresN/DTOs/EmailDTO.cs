using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.DTOs
{
    public class EmailDTO
    {
        [Display(Name = "Correo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debes ingresar un correo válido.")]
        public string Email { get; set; } = null!;
    }
}
