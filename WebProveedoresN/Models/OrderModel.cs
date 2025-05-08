using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class OrderModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Orden de Compra")]
        public int OrderNumber { get; set; }

        [Required]
        [DisplayName("Codigo del proveedor")]
        public string? SupplierCode { get; set; }

        [Required]
        [DisplayName("Fecha")]
        public DateTime OrderDate { get; set; }

        [Required]
        [DisplayName("Importe Total")]
        public decimal TotalAmount { get; set; } = 0;

        [DisplayName("Estado")]
        public string? IdEstatus { get; set; }

        [DisplayName("Moneda")]
        public string DocCurOrder { get; set; } = null!;

        [Display(Name = "Cancelado")]
        public string? Canceled { get; set; }

        [Display(Name = "# de Facturas")]
        public int Invoices { get; set; }

        [Display(Name = "Monto de las Facturas")]
        public decimal TotalInvoice { get; set; } = 0;

        [Display(Name = "Tipo de Documento")]
        public string? DocumentType { get; set; }

        [Display(Name = "Propiedad")]
        public string? Property { get; set; }
    }
}