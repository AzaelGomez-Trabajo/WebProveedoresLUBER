using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class DetailsGoodsReceiptModel
    {
        [Display(Name = "Número de Entrada")]
        public int? DocNum { get; set; }

        [Display(Name = "Factura")]
        public string InvoiceSupplier { get; set; } = null!;

        [Display(Name = "Código de Artículo")]
        public string ItemCode { get; set; } = null!;

        [Display(Name = "Cantidad faltante")]
        public decimal OpenQty { get; set; } = 0;

        [Display(Name = "Estado")]
        public string? DocStatus { get; set; }

        [Display(Name = "Moneda")]
        public string? DocCur { get; set; }

        [Display(Name = "Cantidad")]
        public decimal? Quantity { get; set; } = 0;

        [Display(Name = "Importe")]
        public decimal? Total { get; set; } = 0;

    }
}
