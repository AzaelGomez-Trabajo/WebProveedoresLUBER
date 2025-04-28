using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;

namespace WebProveedoresN.Models
{
    public class OrderDetail : Order
    {
        public string DocumentTypeOrder { get; set; } = null!;

        [Display(Name = "Número de Línea")]
        public int? LineNum { get; set; }

        [Display(Name = "Código de Artículo")]
        public string ItemCode { get; set; } = null!;

        [Display(Name = "Cantidad")]
        public decimal QuantityOrder { get; set; } = 0;

        [Display(Name = "Cantidad faltante")]
        public decimal OpenQty { get; set; } = 0;

        [Display(Name = "Precio por Artículo")]
        public decimal PricePerItem { get; set; }

        [Display(Name = "Total por Artículo")]
        public decimal TotalItem { get; set; }

        [Display(Name = "Impuesto")]
        public decimal Tax { get; set; } = 0;

        [Display(Name = "Total/Impuesto/Artículo")]
        public decimal TotalTaxItem { get; set; }

        [Display(Name = "Faltante")]
        public decimal TotalOrder { get; set; } = 0;

        [Display(Name = "Tipo Documento")]
        public new string? DocumentType { get; set; }

        [Display(Name = "Número de Documento")]
        public int? DocNum { get; set; }

        [Display(Name = "Factura proveedor")]
        public string InvoiceSupplier { get; set; } = null!;

        [Display(Name = "Número de Proveedor")]
        public string? CardCode { get; set; }

        [Display(Name = "Nombre de Proveedor")]
        public string? CardName { get; set; }

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