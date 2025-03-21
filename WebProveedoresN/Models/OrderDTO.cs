using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
	public class OrderDTO
	{
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Orden de Compra")]
        public string OrderNumber { get; set; }
        [Required]
        [DisplayName("Nombre del proveedor")]
        public string SupplierName { get; set; }
        [Required]
        [DisplayName("Fecha de orden")]
        public DateTime OrderDate { get; set; }
        [Required]
        [DisplayName("Monto Total")]
        public decimal TotalAmount { get; set; }
        [DisplayName("Estado")]
        public string Status { get; set; }
        [DisplayName("Moneda")]
        public string Currency {  get; set; }
        public string Canceled { get; set; }
    }
}