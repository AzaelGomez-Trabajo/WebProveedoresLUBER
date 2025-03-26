using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebProveedoresN.Data;
using WebProveedoresN.Interfaces;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class ArchivosController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IIPService _ipService;
        public ArchivosController(IWebHostEnvironment webHostEnvironment, IIPService ipService)
        {
            _webHostEnvironment = webHostEnvironment;
            _ipService = ipService;
        }

        // GET: Archivos/Upload
        public ActionResult Upload(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                return RedirectToAction("ListOrders", "Orders");
            }
            var supplierName = User.FindFirst("SupplierName")?.Value;

            if (string.IsNullOrEmpty(supplierName))
            {
                return RedirectToAction("Login", "Inicio");
            }
            ViewBag.OrderNumber = orderNumber;
            ViewBag.NombreEmpresa = supplierName;
            ViewBag.SupplierId = User.FindFirst("SupplierId")?.Value;
            ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
            var model = new LoadFileDTO
            {
                OrderNumber = orderNumber,
            };
            return View(model);
        }

        // POST: Archivos/Upload
        [HttpPost]
        public ActionResult Upload(LoadFileDTO model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OrderNumber = model.OrderNumber;
                return View(model);
            }
            ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
            var supplierName = User.FindFirst("SupplierName")?.Value;

            try
            {
                var pdfFile = model.FilePDF;
                var xmlFile = model.FileXML;
                var ordenCompra = model.OrderNumber;

                // Validar el tipo de archivo
                if (pdfFile.ContentType != "application/pdf")
                {
                    ModelState.AddModelError("FilePDF", "El archivo debe ser un PDF.");
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
                    return View(model);
                }

                if (xmlFile.ContentType != "text/xml" && xmlFile.ContentType != "application/xml")
                {
                    ModelState.AddModelError("FileXML", "El archivo debe ser un XML.");
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
                    return View(model);
                }

                // Validar el tamaño del archivo
                if (pdfFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("FilePDF", "El archivo PDF no debe exceder los 2 MB.");
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
                    return View(model);
                }

                if (xmlFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("FileXML", "El archivo XML no debe exceder los 2 MB.");
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
                    return View(model);
                }

                // Validar el contenido del archivo XML
                try
                {
                    using (var xmlStream = xmlFile.OpenReadStream())
                    {
                        var xmlDoc = new System.Xml.XmlDocument();
                        xmlDoc.Load(xmlStream);
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("FileXML", "El archivo XML no es válido.");
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
                    return View(model);
                }

                string folderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedFiles");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                string pdfFileName = $"{timestamp}_{ordenCompra}_{Path.GetFileName(pdfFile.FileName)}";
                string xmlFileName = $"{timestamp}_{ordenCompra}_{Path.GetFileName(xmlFile.FileName)}";
                string pdfFilePath = Path.Combine(folderPath, pdfFileName);
                string xmlFilePath = Path.Combine(folderPath, xmlFileName);

                using (var pdfStream = new FileStream(pdfFilePath, FileMode.Create))
                {
                    pdfFile.CopyTo(pdfStream);
                }

                using (var xmlStream = new FileStream(xmlFilePath, FileMode.Create))
                {
                    xmlFile.CopyTo(xmlStream);
                }

                // Obtener el SupplierId del claim
                var supplierIdClaim = User.FindFirst("SupplierId")?.Value;
                if (string.IsNullOrEmpty(supplierIdClaim))
                {
                    // Manejar el caso en que el claim no esté presente
                    return RedirectToAction("Login", "Inicio");
                }
                int supplierId = int.Parse(supplierIdClaim);

                // Leer el contenido del archivo XML
                string xmlContent = System.IO.File.ReadAllText(xmlFilePath);
                string rfcReceptor = XmlServicio.ObtenerRfcReceptor(xmlContent);

                if (!rfcReceptor.Equals("CIN041008173"))
                {
                    ViewBag.Message = "La factura no es para LUBER Lubricantes.";
                    // Eliminar los archivos después de procesarlos
                    System.IO.File.Delete(pdfFilePath);
                    System.IO.File.Delete(xmlFilePath);
                }
                else
                {
                    var pdfName = Path.GetFileNameWithoutExtension(pdfFileName);
                    var xmlName = Path.GetFileNameWithoutExtension(xmlFileName);
                    var archivos = new List<ArchivoDTO>
                            {
                                new() { OrderNumber = ordenCompra, Name = pdfName, Route = folderPath, DateTime = timestamp, Extension = ".pdf", Converted = false },
                                new() { OrderNumber = ordenCompra, Name = xmlName, Route = folderPath, DateTime = timestamp, Extension = ".xml", Converted = false }
                            };

                    XmlServicio.GuardarArchivosEnBaseDeDatos(archivos);
                    var xmlConverted = XmlServicio.ConvertXmlToPdf(xmlContent, Path.Combine(folderPath, $"{timestamp}_1_{xmlName}.pdf"));
                    var archiveConverted = new List<ArchivoDTO>
                        {
                            new() { OrderNumber = ordenCompra, Name = $"{xmlName}", Route = folderPath, DateTime = timestamp, Extension = ".pdf", Converted = true }
                        };
                    XmlServicio.GuardarArchivosEnBaseDeDatos(archiveConverted);

                    var facturaUnica = XmlServicio.BuscarFactura(xmlFilePath);
                    if (facturaUnica)
                    {
                        ViewBag.Message = $"La factura ya ha sido cargada.";
                        // Eliminar los archivos después de procesarlos
                        System.IO.File.Delete(pdfFilePath);
                        System.IO.File.Delete(xmlFilePath);
                        ViewBag.OrderNumber = model.OrderNumber;
                        ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
                        return View(model);

                    }
                        // Guardar los datos del XML en la base de datos
                        XmlServicio.GuardarDatosXmlEnBaseDeDatos(xmlFilePath, ordenCompra, supplierId, supplierName);

                    // Enviar correo de confirmación
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    var nombre = User.FindFirst(ClaimTypes.Name)?.Value;
                    var correo = new CorreoDTO
                    {
                        Para = userEmail,
                        CCO = "noeazael77@hotmail.com",
                        Asunto = "Documentos guardados correctamente",
                        Contenido = $"Hola, {nombre}<br><br>Las facturas: <br><br> {pdfFile.FileName} y <br> {xmlFile.FileName}. <br><br> Para la orden de compra {ordenCompra} de la Empresa {supplierName} se han guardado correctamente.<br><br>Saludos,<br>El equipo de LUBER Lubricantes"
                    };
                    CorreoServicio.EnviarCorreo(correo, nombre);

                    // Redirigir a la vista Index del controlador LecturaXml
                    return RedirectToAction("Index", "LecturaXml", new { xmlFilePath });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Ocurrió un error al procesar los archivos: {ex.Message}";
            }

            ViewBag.OrderNumber = model.OrderNumber;
            ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
            return View(model);
        }

        public async Task<IActionResult> ObtenerDocumentos(string orderNumber, int converted)
        {
            var documents = await DBArchivos.ObtenerDocumentosAsync(orderNumber, converted);
            if (documents != null && documents.Count > 0)
            {
                return Json(new { success = true, documents });
            }
            return Json(new { success = false, message = "No tiene facturas cargadas." });
        }


    }
}