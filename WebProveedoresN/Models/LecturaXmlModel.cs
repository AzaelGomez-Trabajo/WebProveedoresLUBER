using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class LecturaXmlModel
    {
        // Datos del comprobante
        [DisplayName("Codigo de Proveedor")]
        public string SupplierCode { get; set; } = null!;
        [Display(Name = "Folio de la factura")]
        public string Folio { get; set; } = null!;

        [Display(Name = "Serie de la factura")]
        public string Serie { get; set; } = null!;

        [Display(Name = "Version SAT")]
        public double Version { get; set; }

        [Display(Name = "Sello")]
        public string Sello { get; set; } = null!;

        // Datos del proveedor
        [Display(Name = "Nombre Proveedor")]
        public string EmisorNombre { get; set; } = null!;

        [Display(Name = "RFC Proveedor")]
        public string EmisorRfc { get; set; } = null!;

        // RFC a comparar
        [Display(Name = "RFC")]
        public string ReceptorRfc { get; set; } = null!;

        // Datos de los conceptos de la factura
        public List<ConceptModel> Conceptos { get; set; } = [];

        // Importes
        [Display(Name = "Subtotal")]
        public decimal SubTotal { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }

        // Datos de TimbreFiscalDigital
        [Display(Name = "UUID")]
        public string UUID { get; set; } = null!;

    }
}