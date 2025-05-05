using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;

namespace WebProveedoresN.Models
{
    public class OrderDetail
    {
        [Display(Name = "Tipo de Documento")]
        public string DocumentTypeOrder { get; set; } = null!;

        [DisplayName("Orden de Compra")]
        public int OrderNumber { get; set; }
        
        [Display(Name = "Número de Línea")]
        public int? LineNum { get; set; }

        [Display(Name = "Código de Artículo")]
        public string ItemCode { get; set; } = null!;

        [Display(Name = "Cantidad")]
        public decimal QuantityOrder { get; set; } = 0;

        [Display(Name = "Cantidad faltante")]
        public decimal OpenQty { get; set; } = 0;

        [DisplayName("Moneda")]
        public string DocCurOrder { get; set; } = null!;

        [Display(Name = "Impuesto")]
        public decimal Tax { get; set; } = 0;

        [Display(Name = "Precio por Artículo")]
        public decimal PricePerItem { get; set; }

        [Display(Name = "Total por Artículo")]
        public decimal TotalItem { get; set; }

        [Display(Name = "Total/Impuesto/Artículo")]
        public decimal TotalTaxItem { get; set; }

        [Display(Name = "Faltante")]
        public decimal TotalOrder { get; set; } = 0;

        [Display(Name = "Tipo Documento")]
        public string? DocumentType { get; set; }
    }
}