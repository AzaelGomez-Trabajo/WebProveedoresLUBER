using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
	public class OrderDTO
	{
        public int Id { get; set; }
        
        [DisplayName("Orden de Compra")]
        public string? OrderNumber { get; set; }
        
        [DisplayName("Nombre del proveedor")]
        public string? SupplierName { get; set; }
        
        [DisplayName("Fecha de orden")]
        public DateTime OrderDate { get; set; }
        
        [DisplayName("Monto Total")]
        public decimal TotalAmount { get; set; }
        
        [DisplayName("Estado")]
        public string? IdEstatus { get; set; }
        
        [DisplayName("Moneda")]
        public string? Currency {  get; set; }
        
        public string? Canceled { get; set; }
        
        public int Invoices { get; set; }
        
        public decimal TotalInvoice { get; set; }

        public string? DocumentType { get; set; }

        public string? Property { get; set; }
    }
}