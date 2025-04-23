using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebProveedoresN.Data;
using WebProveedoresN.Interfaces;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    [Authorize(Roles = "Administrador, Usuario")]
    public class FilesController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IIPService _ipService;

        public FilesController(IWebHostEnvironment webHostEnvironment, IIPService ipService)
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
                return RedirectToAction("Login", "Access");
            }
            ViewBag.OrderNumber = orderNumber;
            ViewBag.NombreEmpresa = supplierName;
            ViewBag.SupplierCode = User.FindFirst("SupplierCode")?.Value;
            ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
            //ViewBag.OrderId = orderId;
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
                //ViewBag.OrderId = orderId;
                ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
                return View(model);
            }
            ViewBag.UserIpAddress = _ipService.GetUserIpAddress();
            var supplierName = User.FindFirst("SupplierName")?.Value;
            // Obtener el SupplierId del claim
            var supplierCodeClaim = User.FindFirst("SupplierCode")?.Value;
            if (string.IsNullOrEmpty(supplierCodeClaim))
            {
                // Manejar el caso en que el claim no esté presente
                return RedirectToAction("Login", "Access");
            }
            var supplierCode = supplierCodeClaim;
            var idUsuario = User.FindFirst("IdUsuario")?.Value;
            var ipAddress = _ipService.GetUserIpAddress();
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
                    ViewBag.UserIpAddress = ipAddress;
                    return View(model);
                }

                if (xmlFile.ContentType != "text/xml" && xmlFile.ContentType != "application/xml")
                {
                    ModelState.AddModelError("FileXML", "El archivo debe ser un XML.");
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = ipAddress;
                    return View(model);
                }

                // Validar el tamaño del archivo
                if (pdfFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("FilePDF", "El archivo PDF no debe exceder los 2 MB.");
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = ipAddress;
                    return View(model);
                }

                if (xmlFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("FileXML", "El archivo XML no debe exceder los 2 MB.");
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = ipAddress;
                    return View(model);
                }

                // Validar el contenido del archivo XML
                var xmlContent = string.Empty;
                try
                {
                    using (var xmlStream = xmlFile.OpenReadStream())
                    {
                        var xmlDoc = new System.Xml.XmlDocument();
                        xmlDoc.Load(xmlStream);
                        xmlContent = xmlDoc.InnerXml;
                    }
                }
                catch
                {
                    ModelState.AddModelError("FileXML", "El archivo XML no es válido.");
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = ipAddress;
                    return View(model);
                }

                // Leer el contenido del archivo XML
                var rfcReceptor = string.Empty;
                var datos = XmlServicio.GetDataFromXml(xmlContent);
                foreach (var dato in datos)
                {
                    dato.SupplierCode = supplierCode;
                    rfcReceptor = dato.ReceptorRfc;
                }

                if (!rfcReceptor.Equals("CIN041008173"))
                {
                    ViewBag.Message = "La factura no es para LUBER Lubricantes.";
                }

                var facturaUnica = XmlServicio.SearchInvoice(datos[0].UUID);
                if (facturaUnica)
                {
                    ViewBag.Message = $"La factura ya ha sido cargada.";
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = ipAddress;
                    return View(model);
                }

                var estadoCFDI = XmlServicio.GetCFDIStatus(datos[0].EmisorRfc, datos[0].ReceptorRfc, datos[0].Total.ToString(), datos[0].UUID, datos[0].Sello);
                var estado = string.Empty;
                foreach (var xml in estadoCFDI)
                {
                    //estado = xml.Estado.ToString();
                    estado = "Vigente";
                }
                if (estado != "Vigente")
                {
                    ViewBag.Message = $"El CFDI no está vigente. Estado: {estado}.";
                    ViewBag.OrderNumber = model.OrderNumber;
                    ViewBag.UserIpAddress = ipAddress;
                    return View(model);
                }
                // Guardar los datos del XML en la base de datos
                var result = XmlServicio.SaveXmlDataInDatabase(datos, ordenCompra, supplierName, idUsuario, ipAddress);

                if (result != "OK")
                {
                    ViewBag.Message = $"{result} {ordenCompra}.";
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

                var pdfName = Path.GetFileNameWithoutExtension(pdfFileName);
                var xmlName = Path.GetFileNameWithoutExtension(xmlFileName);
                var xmlConverted = XmlServicio.ConvertXmlToPdf(xmlContent, Path.Combine(folderPath, $"{timestamp}_1_{xmlName}.pdf"));
                var archivos = new List<FileDTO>
                            {
                                new() { OrderNumber = model.OrderNumber, Name = pdfName, Route = folderPath, DateTime = timestamp, Extension = ".pdf", Converted = false },
                                new() { OrderNumber = model.OrderNumber, Name = xmlName, Route = folderPath, DateTime = timestamp, Extension = ".xml", Converted = false },
                                new() { OrderNumber = model.OrderNumber, Name = xmlName, Route = folderPath, DateTime = timestamp, Extension = ".pdf", Converted = true }
                            };
                XmlServicio.SaveFilesToDatabase(archivos);

                // Enviar correo de confirmación
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var nombre = User.FindFirst(ClaimTypes.Name)?.Value;
                var correo = new EmalDTO
                {
                    Para = userEmail,
                    CCO = "noeazael77@hotmail.com",
                    Asunto = "Documentos guardados correctamente",
                    Contenido = $"Hola, {nombre}<br><br>Las facturas: <br><br> {pdfFile.FileName} y <br> {xmlFile.FileName}. <br><br> Para la orden de compra {ordenCompra} de la Empresa {supplierName} se han guardado correctamente.<br><br>Saludos,<br>El equipo de LUBER Lubricantes"
                };
                CorreoServicio.EnviarCorreo(correo, nombre);

                // Redirigir a la lista de órdenes
                return RedirectToAction("Index", "Lecturaxml", new { xmlContent });
                //return RedirectToAction("ListOrders", "Orders");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Ocurrió un error al procesar los archivos: {ex.Message}";
            }
            ViewBag.UserIpAddress = ipAddress;
            ViewBag.OrderNumber = model.OrderNumber;
            //ViewBag.OrderId = orderId;
            return View(model);
        }

        public async Task<IActionResult> ObtenerDocumentos(string orderNumber)
        {
            var documents = await DBFiles.ObtenerDocumentosAsync(orderNumber);
            if (documents != null && documents.Count > 0)
            {
                return Json(new { success = true, documents });
            }
            return Json(new { success = false, message = $"No tiene facturas cargadas la Orden de Compra." });
        }

        [HttpPost]
        public IActionResult CerrarEmbed()
        {
            return Json(new { success = true });
        }
    }
}