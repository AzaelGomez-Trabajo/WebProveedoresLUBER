using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class Order
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
        [DisplayName("Fecha de orden")]
        public DateTime OrderDate { get; set; }

        [Required]
        [DisplayName("Monto Total")]
        public decimal TotalAmount { get; set; } = 0;

        [DisplayName("Estado")]
        public string? IdEstatus { get; set; }

        [DisplayName("Moneda")]
        public string DocCurOrder { get; set; } = null!;

        public string? Canceled { get; set; }
        public int Invoices { get; set; }
        public decimal TotalInvoice { get; set; } = 0;
        public string? DocumentType { get; set; }
        public string? Property { get; set; }
    }
}