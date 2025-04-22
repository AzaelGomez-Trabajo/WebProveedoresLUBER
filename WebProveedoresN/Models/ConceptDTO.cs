using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class ConceptDTO
    {
        [Display(Name = "Cantidad")]
        public decimal Cantidad { get; set; }

        [Display(Name = "Descripcion")]
        public string Descripcion { get; set; }

        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        [Display(Name = "Valor Unitario")]
        public decimal ValorUnitario { get; set; }

    }
}
