using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Data;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class ArchivosController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ArchivosController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        ////GET: Archivos/OrderNumber
        //public ActionResult OrderNumber()
        //{
        //    //if (Session["Empresa"] == null || string.IsNullOrEmpty(Session["Empresa"].ToString()))
        //    //{
        //    return RedirectToAction("Login", "Inicio");
        //    //}
        //    ViewBag.NombreEmpresa = Session["Empresa"];
        //    var model = new OrderDTO { NombreEmpresa = ViewBag.NombreEmpresa.ToString() };
        //    return View(model);
        //}

        ////POST: Archivos/ValidateOrderNumber
        //   [HttpPost]
        //    public ActionResult ValidateOrderNumber(OrderDTO order)
        //{
        //    if (Session["Empresa"] == null || string.IsNullOrEmpty(Session["Empresa"].ToString()))
        //    {
        //        return RedirectToAction("Login", "Inicio");
        //    }
        //    order.NombreEmpresa = Session["Empresa"].ToString();
        //    ViewBag.NombreEmpresa = order.NombreEmpresa;
        //    ViewBag.OrderNumber = order.OrderNumber;


        //    bool isValidOrder = DBArchivos.ValidateOrderNumberInDatabase(order);

        //    if (isValidOrder)
        //    {
        //        return RedirectToAction("Upload", order);
        //    }
        //    else
        //    {
        //        ViewBag.Message = "Número de orden de compra no válido o no pertenece al proveedor especificado.";
        //        return View("OrderNumber", order);
        //    }
        //}

        // GET: Archivos/Upload
        public ActionResult Upload(OrderDTO order)
        {
            if (string.IsNullOrEmpty(order.OrderNumber))
            {
                return RedirectToAction("OrderNumber");
            }
            if (string.IsNullOrEmpty(order.SupplierName))
            {
                return RedirectToAction("Login", "Inicio");
            }
            ViewBag.OrderNumber = order.OrderNumber;
            ViewBag.NombreEmpresa = order.SupplierName;
            var model = new LoadFileDTO
            {
                OrderNumber = order.OrderNumber,
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

            try
            {
                var pdfFile = model.FilePDF;
                var xmlFile = model.FileXML;
                var ordenCompra = model.OrderNumber;

                string folderPath = Path.Combine(_webHostEnvironment.ContentRootPath,"UploadedFiles");
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

                // Leer el contenido del archivo XML
                string xmlContent = System.IO.File.ReadAllText(xmlFilePath);
                string rfcReceptor = XmlServicio.ObtenerRfcReceptor(xmlContent);

                if (!rfcReceptor.Equals("CIN041008173"))
                {
                    ViewBag.Message = "La factura no es para LUBER Lubricantes.";
                }
                else
                {

                    var archivos = new List<ArchivoDTO>
                        {
                            new ArchivoDTO { Nombre = pdfFileName, Ruta = pdfFilePath, FechaHora = timestamp },
                            new ArchivoDTO { Nombre = xmlFileName, Ruta = xmlFilePath, FechaHora = timestamp }
                        };

                    XmlServicio.GuardarArchivosEnBaseDeDatos(archivos);
                    XmlServicio.ConvertXmlToPdf(xmlContent, Path.Combine(folderPath, $"{timestamp}_Converted_{Path.GetFileNameWithoutExtension(xmlFileName)}.pdf"));

                    XmlServicio.GuardarDatosXmlEnBaseDeDatos(xmlFilePath);

                    // Enviar correo de confirmación
                    var correo = new CorreoDTO
                    {
                        Para = "programador1@luberoil.com",
                        Asunto = "Documentos guardados correctamente",
                        Contenido = $"Hola, Azael<br><br>Los documentos para la orden de compra {ordenCompra} de la Empresa {TempData["Empresa"]} se han guardado correctamente.<br><br>Saludos,<br>El equipo de LUBER Lubricantes"
                    };
                    CorreoServicio.EnviarCorreo(correo);

                    // Redirigir a la vista Index del controlador LecturaXml
                    return RedirectToAction("Index", "LecturaXml", new { xmlFilePath });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Ocurrió un error al procesar los archivos: {ex.Message}";
            }

            ViewBag.OrderNumber = model.OrderNumber;
            return View();
        }

            //// POST: Archivos/ValidateRfcReceptor
            //[HttpPost]
            //public JsonResult ValidateRfc(string xmlContent)
            //{
            //    string rfcReceptor = XmlServicio.ObtenerRfcReceptor(xmlContent);
            //    if (!string.IsNullOrEmpty(rfcReceptor))
            //    {
            //        return Json(new { success = true, message = $"RFC del Receptor: {rfcReceptor}" });
            //    }
            //    else
            //    {
            //        return Json(new { success = false, message = "No se pudo obtener el RFC del receptor." });
            //    }
            //}


    }
}