using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class DetailsGoodsReceipt
    {
        [Display(Name = "Número de Documento")]
        public int? DocNum { get; set; }

        [Display(Name = "Factura proveedor")]
        public string InvoiceSupplier { get; set; } = null!;

        [Display(Name = "Código de Artículo")]
        public string ItemCode { get; set; } = null!;

        [Display(Name = "Cantidad faltante")]
        public decimal OpenQty { get; set; } = 0;

        [Display(Name = "Estado del Documento")]
        public string? DocStatus { get; set; }

        [Display(Name = "Moneda")]
        public string? DocCur { get; set; }

        [Display(Name = "Cantidad Cubierta")]
        public decimal? Quantity { get; set; } = 0;

        [Display(Name = "Faltante")]
        public decimal? Total { get; set; } = 0;

    }
}
