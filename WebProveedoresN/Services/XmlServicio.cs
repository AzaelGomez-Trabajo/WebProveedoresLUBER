using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Xml;
using System.Xml.Linq;
using WebProveedoresN.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public class XmlServicio
    {
        public static string ObtenerRfcReceptor(string xmlContent)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");

            var receptorNode = xmlDoc.SelectSingleNode("//cfdi:Receptor", nsmgr);
            if (receptorNode != null && receptorNode.Attributes["Rfc"] != null)
            {
                return receptorNode.Attributes["Rfc"].Value;
            }

            return null;
        }

        public static List<LecturaXmlDTO> ObtenerDatosDesdeXml(string rutaXml)
        {
            var archivos = new List<LecturaXmlDTO>();
            try
            {
                if (File.Exists(rutaXml))
                {
                    var xmlDoc = XDocument.Load(rutaXml);
                    foreach (var element in xmlDoc.Descendants("{http://www.sat.gob.mx/cfd/4}Comprobante"))
                    {
                        var archivo = new LecturaXmlDTO
                        {
                            // Comprobante
                            FolioFactura = element.Attribute("Folio")?.Value,
                            Version = double.Parse(element.Attribute("Version")?.Value ?? "0"),
                            Serie = element.Attribute("Serie")?.Value,
                            Sello = element.Attribute("Sello")?.Value,
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
                            var concepto = new ConceptoDTO()
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
                else
                {
                    throw new FileNotFoundException("El archivo XML no se encontró en la ruta especificada.");
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

        public static void GuardarArchivosEnBaseDeDatos(List<ArchivoDTO> archivos)
        {
            foreach (var archivo in archivos)
            {
                DBArchivos.SaveFileToDatabase(archivo);
            }
        }

        public static void GuardarDatosXmlEnBaseDeDatos(string xmlFilePath, string orderNumber, int supplierId, string supplierName)
        {
            var archivos = ObtenerDatosDesdeXml(xmlFilePath);
            foreach(var archivo in archivos)
            {
                archivo.SupplierId = supplierId;
            }
            DBArchivos.GuardarDatosEnSqlServer(archivos, orderNumber, supplierName);
        }

        public static bool BuscarFactura(string xmlFilePath)
        {
            var archivos = ObtenerDatosDesdeXml(xmlFilePath);
            foreach (var archivo in archivos)
            {
                if (DBArchivos.BuscarFactura(archivo))
                {
                    return true;
                }
            }
            return false;
        }
    }
}