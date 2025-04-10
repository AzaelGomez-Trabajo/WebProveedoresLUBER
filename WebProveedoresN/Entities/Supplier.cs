using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Entities
{
    public class Supplier
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [DisplayName("Codigo de Proveedor")]
        public string Code { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [DisplayName("Proveedor")]
        public string Name { get; set; }

    }
}
