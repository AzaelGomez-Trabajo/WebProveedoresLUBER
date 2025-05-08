using iTextSharp.text;
using iTextSharp.text.pdf;
using SAT.Services.ConsultaCFDIService;
using SW.Services.Status;
using System.Xml;
using System.Xml.Linq;
using WebProveedoresN.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Repositories.Implementations
{
    public class XmlRepository
    {
        public static List<LecturaXmlModel> GetDataFromXml(string xmlContent)
        {
            var archivos = new List<LecturaXmlModel>();
            try
            {
                    var xmlDoc = XDocument.Parse(xmlContent);
                    foreach (var element in xmlDoc.Descendants("{http://www.sat.gob.mx/cfd/4}Comprobante"))
                    {
                        var archivo = new LecturaXmlModel
                        {
                            // Comprobante
                            Folio = element.Attribute("Folio")?.Value ?? "0",
                            Version = double.Parse(element.Attribute("Version")?.Value ?? "0"),
                            Serie = element.Attribute("Serie")?.Value ?? "0",
                            Sello = element.Attribute("Sello")?.Value ?? "0",
                            // Provedor
                            EmisorNombre = element.Element("{http://www.sat.gob.mx/cfd/4}Emisor")?.Attribute("Nombre")?.Value,
                            EmisorRfc = element.Element("{http://www.sat.gob.mx/cfd/4}Emisor")?.Attribute("Rfc")?.Value,
                            // RFC a comparar
                            ReceptorRfc = element.Element("{http://www.sat.gob.mx/cfd/4}Receptor")?.Attribute("Rfc")?.Value,
                            // Importes
                            SubTotal = decimal.Parse(element.Attribute("SubTotal")?.Value ?? "0"),
                            Total = decimal.Parse(element.Attribute("Total")?.Value ?? "0"),
                            UUID = element.Element("{http://www.sat.gob.mx/cfd/4}Complemento")?.Element("{http://www.sat.gob.mx/TimbreFiscalDigital}TimbreFiscalDigital")?.Attribute("UUID")?.Value
                        };
                        // Conceptos

                        foreach (var conceptoElement in element.Descendants("{http://www.sat.gob.mx/cfd/4}Concepto"))
                        {
                            var concepto = new ConceptModel()
                            {
                                Cantidad = decimal.Parse(conceptoElement.Attribute("Cantidad")?.Value ?? "0"),
                                Descripcion = conceptoElement.Attribute("Descripcion")?.Value,
                                ValorUnitario = decimal.Parse(conceptoElement.Attribute("ValorUnitario")?.Value ?? "0"),
                                Importe = decimal.Parse(conceptoElement.Attribute("Importe")?.Value ?? "0")
                            };
                            archivo.Conceptos.Add(concepto);
                        }

                        archivos.Add(archivo);
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el archivo XML {ex.Message}");
            }
            return archivos;
        }

        public static string ConvertXmlToPdf(string xmlContent, string pdfFilePath)
        {
            var document = new Document();
            using (var fs = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                // Agregar contenido del XML al PDF
                document.Add(new Paragraph(xmlContent));

                document.Close();
                writer.Close();
            }
            return document.ToString();
        }

        public static void SaveFilesToDatabase(List<FileModel> archivos)
        {
            foreach (var archivo in archivos)
            {
                DBFiles.SaveFileToDatabase(archivo);
            }
        }

        public static string SaveXmlDataInDatabase(List<LecturaXmlModel> archivos, string orderNumber, string supplierName, string idUsuario, string ipUsuario) => DBFiles.SaveXmlDataInDatabase(archivos, orderNumber, supplierName, idUsuario, ipUsuario);
        

        public static bool SearchInvoice(string UUID)
        {
            return DBFiles.BuscarFactura(UUID);
        }
    
        public static List<CFDIModel> GetCFDIStatus(string RfcEmisor, string RfcReceptor, string Total, string UUID, string Sello)
        {
            var xml = new CFDIModel();
            // Verificar el estado del CFDI
            Status status = new Status("https://consultaqr.facturaelectronica.sat.gob.mx/ConsultaCFDIService.svc");
            Acuse response = status.GetStatusCFDI(RfcEmisor, RfcReceptor, Total, UUID, Sello);
            xml.Estado = response.Estado;
            xml.Codigo_Estatus = response.CodigoEstatus;
            xml.Es_Cancelable = response.EsCancelable;
            xml.Cancelacion_Estatus = response.EstatusCancelacion;
            return [xml];
        }
    }
}