using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class OrderDetailsOffer : Order
    {
        public string Status { get; set; } = null!;
        public int LineNum { get; set; }
        public string ItemCode { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal OpenQty { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalItem { get; set; }
        public decimal TotalTax { get; set; }
    }
}
