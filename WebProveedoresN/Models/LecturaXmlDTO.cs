using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class LecturaXmlDTO
    {
        // Datos del comprobante
        [DisplayName("Codigo de Proveedor")]
        public string SupplierCode { get; set; }
        [Display(Name = "Folio de la factura")]
        public string Folio { get; set; }
        
        [Display(Name = "Serie de la factura")]
        public string Serie { get; set; }
        
        [Display(Name = "Version SAT")]
        public double Version { get; set; }

        [Display(Name = "Sello")]
        public string Sello { get; set; }

        // Datos del proveedor
        [Display(Name = "Nombre Proveedor")]
        public string EmisorNombre { get; set; }

        [Display(Name = "RFC Proveedor")]
        public string EmisorRfc { get; set; }

        // RFC a comparar
        [Display(Name = "RFC")]
        public string ReceptorRfc { get; set; }

        // Datos de los conceptos de la factura
        public List<ConceptoDTO> Conceptos { get; set; } = [];

        // Importes
        [Display(Name = "Subtotal")]
        public decimal SubTotal { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }

        // Datos de TimbreFiscalDigital
        [Display(Name = "UUID")]
        public string UUID { get; set; }

    }
}