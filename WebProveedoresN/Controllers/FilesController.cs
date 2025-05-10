using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebProveedoresN.Data;
using WebProveedoresN.DTOs;
using WebProveedoresN.Models;
using WebProveedoresN.Repositories.Implementations;
using WebProveedoresN.Repositories.Interfaces;

namespace WebProveedoresN.Controllers
{
    [Authorize(Roles = "Administrador, Usuario")]
    public class FilesController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IIPRepository _ipService;
        private readonly IOrderRepository _orderRepository;
        private readonly IFilesRepository _filesRepository;
        private readonly IEmailRepository _emailRepository;

        public FilesController(IWebHostEnvironment webHostEnvironment, IIPRepository ipService, IOrderRepository orderRepository, IFilesRepository filesRepository, IEmailRepository emailRepository)
        {
            _webHostEnvironment = webHostEnvironment;
            _ipService = ipService;
            _orderRepository = orderRepository;
            _filesRepository = filesRepository;
            _emailRepository = emailRepository;
        }

        [HttpPost("Upload2")]
        public IActionResult Upload2(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                return BadRequest("Faltan datos necesarios para procesar la solicitud.");
            }
            // Guardar el número de orden en TempData
            TempData["OrderNumber"] = orderNumber;
            // Redirigir a la vista de carga de archivos
            try
            {
                return Redirect("/Files/UploadOffer");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error en la redireccion: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult UploadOffer()
        {
            var orderNumber = TempData["OrderNumber"] as string;
            if (string.IsNullOrEmpty(orderNumber))
            {
                return BadRequest("No se encontraron datos para procesar.");
            }
            ViewBag.OrderNumber = orderNumber;
            ViewBag.SupplierCode = User.FindFirst("SupplierCode")?.Value;
            // Pasar los datos a la vista
            var model = new LoadFileModel
            {
                OrderNumber = int.Parse(orderNumber),
            };
            return View(model);
        }

        [HttpPost("UploadOffer")]
        public async Task<IActionResult> UploadOffer(LoadFileModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var supplierName = User.FindFirst("SupplierName")?.Value;
            var supplierCode = GetClaimValue("SupplierCode");
            var idUsuario = User.FindFirst("IdUsuario")?.Value;
            var ipAddress = _ipService.GetUserIpAddress();

            if (string.IsNullOrEmpty(supplierCode))
            {
                // Manejar el caso en que el claim no esté presente
                return RedirectToAction("Login", "Access");
            }

            try
            {
                // Validar archivos
                var ValidationResult = ValidateFiles(model);
                if (ValidationResult != null)
                {
                    return ValidationResult;
                }

                // Leer el contenido del archivo XML
                var xmlContent = ReadXmlContent(model.FileXML);
                if(xmlContent == null)
                {
                    ModelState.AddModelError("FileXML", "El archivo XML no es válido.");
                    return View(model);
                }

                // Procesar datos del XML
                var datos = _filesRepository.GetDataFromXml(xmlContent);
                var validationMessage = ValidateXmlData(datos, supplierCode);
                if (validationMessage != null)
                {
                    ViewBag.Message = validationMessage;
                    return View(model);
                }

                // Validar el estado del CFDI
                var estadoCFDI = await ValidateCFDIStatus(datos[0]);
                if (estadoCFDI != "Vigente")
                {
                    ViewBag.Message = $"El CFDI no está vigente. Estado: {estadoCFDI}.";
                    return View(model);
                }

                // Guardar los datos del XML en la base de datos
                var result = await _filesRepository.SaveXmlDataInDatabaseAsync(datos, model.OrderNumber, supplierName!, idUsuario!, ipAddress);
                if (result != "OK")
                {
                    ViewBag.Message = $"{result} {model.OrderNumber}.";
                    return View(model);
                }

                // Guardar archivos en el sistema
                SaveFilesToSystem(model, xmlContent);

                // Enviar correo de confirmación
                SendConfirmationEmail(model, supplierName!);

                // Redirigir a la lista de órdenes
                return RedirectToAction("Index", "Lecturaxml", new { xmlContent });
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Ocurrió un error al processar los archivos: {ex.Message}";
                return View(model);
            }

            //try
            //{
            //    var pdfFile = model.FilePDF!;
            //    var xmlFile = model.FileXML!;
            //    var orderNumber = model.OrderNumber!;

            //    // Validar el tipo de archivo
            //    if (pdfFile.ContentType != "application/pdf")
            //    {
            //        ModelState.AddModelError("FilePDF", "El archivo debe ser un PDF.");
            //        return View(model);
            //    }

            //    if (xmlFile.ContentType != "text/xml" && xmlFile.ContentType != "application/xml")
            //    {
            //        ModelState.AddModelError("FileXML", "El archivo debe ser un XML.");
            //        return View(model);
            //    }

            //    // Validar el tamaño del archivo
            //    if (pdfFile.Length > 2 * 1024 * 1024)
            //    {
            //        ModelState.AddModelError("FilePDF", "El archivo PDF no debe exceder los 2 MB.");
            //        return View(model);
            //    }

            //    if (xmlFile.Length > 2 * 1024 * 1024)
            //    {
            //        ModelState.AddModelError("FileXML", "El archivo XML no debe exceder los 2 MB.");
            //        return View(model);
            //    }

            //    // Validar el contenido del archivo XML
            //    var xmlContent = string.Empty;
            //    try
            //    {
            //        using (var xmlStream = xmlFile.OpenReadStream())
            //        {
            //            var xmlDoc = new System.Xml.XmlDocument();
            //            xmlDoc.Load(xmlStream);
            //            xmlContent = xmlDoc.InnerXml;
            //        }
            //    }
            //    catch
            //    {
            //        ModelState.AddModelError("FileXML", "El archivo XML no es válido.");
            //        return View(model);
            //    }

            //    // Leer el contenido del archivo XML
            //    var rfcReceptor = string.Empty;
            //    var invoice = string.Empty;
            //    var datos = _filesRepository.GetDataFromXml(xmlContent);
            //    foreach (var dato in datos)
            //    {
            //        dato.SupplierCode = supplierCode;
            //        rfcReceptor = dato.ReceptorRfc;
            //        invoice = dato.Folio;
            //    }

            //    if (!rfcReceptor.Equals("CIN041008173"))
            //    {
            //        ViewBag.Message = "La factura no es para LUBER Lubricantes.";
            //        return View(model);
            //    }

            //    var facturaUnica = await _filesRepository.SearchInvoiceAsync(datos[0].UUID);
            //    if (facturaUnica)
            //    {
            //        ViewBag.Message = $"La factura ya ha sido cargada.";
            //        return View(model);
            //    }

            //    var cFDIStatusModel = new CFDIStatusModel
            //    {
            //        EmisorRFC = datos[0].EmisorRfc,
            //        ReceptorRFC = datos[0].ReceptorRfc,
            //        Total = datos[0].Total.ToString(),
            //        UUID = datos[0].UUID,
            //        Sello = datos[0].Sello
            //    };

            //    var estadoCFDI = await _filesRepository.GetCFDIStatusAsync(cFDIStatusModel);
            //    var estado = string.Empty;
            //    foreach (var xml in estadoCFDI)
            //    {
            //        //estado = xml.Estado.ToString();
            //        estado = "Vigente";
            //    }
            //    if (estado != "Vigente")
            //    {
            //        ViewBag.Message = $"El CFDI no está vigente. Estado: {estado}.";
            //        return View(model);
            //    }
            //    // Guardar los datos del XML en la base de datos
            //    var result = await _filesRepository.SaveXmlDataInDatabaseAsync(datos, orderNumber, supplierName!, idUsuario!, ipAddress);

            //    if (result != "OK")
            //    {
            //        ViewBag.Message = $"{result} {orderNumber}.";
            //        return View(model);
            //    }

            //    string folderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedFiles");
            //    if (!Directory.Exists(folderPath))
            //    {
            //        Directory.CreateDirectory(folderPath);
            //    }

            //    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            //    string pdfFileName = $"{timestamp}_{orderNumber}_{Path.GetFileName(pdfFile.FileName)}";
            //    string xmlFileName = $"{timestamp}_{orderNumber}_{Path.GetFileName(xmlFile.FileName)}";
            //    string pdfFilePath = Path.Combine(folderPath, pdfFileName);
            //    string xmlFilePath = Path.Combine(folderPath, xmlFileName);

            //    using (var pdfStream = new FileStream(pdfFilePath, FileMode.Create))
            //    {
            //        pdfFile.CopyTo(pdfStream);
            //    }

            //    using (var xmlStream = new FileStream(xmlFilePath, FileMode.Create))
            //    {
            //        xmlFile.CopyTo(xmlStream);
            //    }

            //    var pdfName = Path.GetFileNameWithoutExtension(pdfFileName);
            //    var xmlName = Path.GetFileNameWithoutExtension(xmlFileName);
            //    var xmlConverted = _filesRepository.ConvertXmlToPdf(xmlContent, Path.Combine(folderPath, $"{timestamp}_1_{xmlName}.pdf"));
            //    var archivos = new List<FileModel>
            //                {
            //                    new() { OrderNumber = orderNumber, Name = pdfName, Route = folderPath, DateTime = timestamp, Extension = ".pdf", Converted = false },
            //                    new() { OrderNumber = orderNumber, Name = xmlName, Route = folderPath, DateTime = timestamp, Extension = ".xml", Converted = false },
            //                    new() { OrderNumber = orderNumber, Name = xmlName, Route = folderPath, DateTime = timestamp, Extension = ".pdf", Converted = true }
            //                };
            //    await _filesRepository.SaveFilesToDatabaseAsync(archivos);

            //    // Enviar correo de confirmación
            //    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value!;
            //    var nombre = User.FindFirst(ClaimTypes.Name)?.Value!;
            //    var correo = new EmailModel
            //    {
            //        Para = userEmail,
            //        CCO = "noeazael77@hotmail.com",
            //        Asunto = "Documentos guardados correctamente",
            //        Contenido = $"Hola, {nombre}<br><br>Las facturas: <br><br> {pdfFile.FileName} y <br> {xmlFile.FileName}. <br><br> Para la orden de compra {orderNumber} de la Empresa {supplierName} se han guardado correctamente.<br><br>Saludos,<br>El equipo de LUBER Lubricantes"
            //    };
            //    EmailRepository.EnviarCorreo(correo, nombre);

            //    // Redirigir a la lista de órdenes
            //    return RedirectToAction("Index", "Lecturaxml", new { xmlContent });
            //}
            //catch (Exception ex)
            //{
            //    ViewBag.Message = $"Ocurrió un error al procesar los archivos: {ex.Message}";
            //}
            //return View(model);
        }

        [HttpPost("Upload")]
        public IActionResult Upload(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                return BadRequest("Faltan datos necesarios para procesar la solicitud.");
            }

            // Guardar los parámetros en TempData
            TempData["OrderNumber"] = orderNumber;

            // Redirigir a la vista de carga de archivos
            try
            {
                return Redirect("/Files/UploadSaves");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error en la redireccion: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult UploadSaves()
        {
            var orderNumber = TempData["OrderNumber"] as string;

            if (string.IsNullOrEmpty(orderNumber))
            {
                return BadRequest("No se encontraron datos para procesar.");
            }

            ViewBag.OrderNumber = orderNumber;
            ViewBag.SupplierCode = GetClaimValue("SupplierCode");

            // Pasar los datos a la vista
            var model = new LoadFileModel
            {
                //OrderNumber = int.Parse(orderNumber),
                OrderNumber = int.Parse(ViewBag.OrderNumber),
            };

            return View(model);
        }

        // POST: Files/UploadSaves
        [HttpPost("UploadSaves")]
        public async Task<IActionResult> UploadSaves(LoadFileModel model)
        {
            ViewBag.Message = null;
            if (!ModelState.IsValid)
            {
                ViewBag.OrderNumber = model.OrderNumber;
                return View();
            }

            var supplierName = GetClaimValue("SupplierName")!;
            var supplierCode = GetClaimValue("SupplierCode");
            var idUsuario = GetClaimValue("IdUsuario");
            var ipAddress = _ipService.GetUserIpAddress();
            if (string.IsNullOrEmpty(supplierCode))
            {
                // Manejar el caso en que el claim no esté presente
                return RedirectToAction("Login", "Access");
            }

            try
            {
                // Validar Archivos
                var validationResult = ValidateFiles(model);
                if (validationResult != null)
                {
                    return validationResult;
                }

                //var orderDatailsDTO = new OrderDetailsDTO
                //{
                //    OrderNumber = model.OrderNumber,
                //    SupplierCode = supplierCode,
                //};
                var orderDetailDTO = new OrderDetailDTO
                {
                    Action = 3,
                    OrderNumber = model.OrderNumber,
                    SupplierCode = supplierCode,
                    DocumentType = ""
                };

                // Validar la orden de compra
                var orderDetails = await GetOrderDetailsAsync(orderDetailDTO);
                if (orderDetails! == null || orderDetails.Count == 0)
                {
                    ViewBag.Message = $"La orden de compra {model.OrderNumber} no tiene una entrada de mercancía generada.";
                    return View(model);
                }

                // Leer el contenido del archivo XML
                var xmlContent = ReadXmlContent(model.FileXML);
                if (xmlContent == null)
                {
                    ModelState.AddModelError("FileXML", "El archivo XML no es válido.");
                    return View(model);
                }

                // Procesar datos del XML
                var datos = _filesRepository.GetDataFromXml(xmlContent);
                var validationMessage = await ValidateXmlData(datos, supplierCode, orderDetails);
                if (validationMessage != null)
                {
                    ViewBag.Message = validationMessage!;
                    return View(model);
                }

                // Validar el estado del CFDI
                var CFDIStatus = await ValidateCFDIStatus(datos[0]);
                CFDIStatus = "Vigente";
                if (CFDIStatus != "Vigente")
                {
                    ViewBag.Message = $"El CFDI no está vigente. Estado: {CFDIStatus}.";
                    return View(model);
                }

                // Guardar los datos del XML en la base de datos
                var result = await _filesRepository.SaveXmlDataInDatabaseAsync(datos, model.OrderNumber, supplierName, idUsuario!, ipAddress);
                if (result != "OK")
                {
                    ViewBag.Message = $"{result} {model.OrderNumber}.";
                    return View(model);
                }

                // Guardar archivos en el sistema
                await SaveFilesToSystem(model, xmlContent);

                // Enviar correo de confirmación
                var resultSendEmail = SendConfirmationEmail(model, supplierName);

                if (resultSendEmail)
                {
                    Console.WriteLine("Correo enviado exitosamente");
                }
                else
                {
                    Console.WriteLine("Error al enviar el correo");
                }
                    // Redirigir a la lista de órdenes
                    return RedirectToAction("Index", "Lecturaxml", new { xmlContent });
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Ocurrió un error al procesar los archivos: {ex.Message}";
                ViewBag.OrderNumber = model.OrderNumber;
                return View(model);
            }
            //// Cargo la informacion para obtener la orden de compra
            //var orderDetailDTO = new OrderDetailDTO
            //{
            //    Action = 2,
            //    OrderNumber = model.OrderNumber,
            //    SupplierCode = User.FindFirst("SupplierCode")!.Value,
            //    DocumentType = ""
            //};
            //// Obtengo la información de la orden de compar para saber el tipo de documento
            //var order = await _orderService.GetOrderByOrderNumberAsync(orderDetailDTO);
            //var orderDetailsDTO = new OrderDetailDTO
            //{
            //    Action = 3,
            //    OrderNumber = model.OrderNumber,
            //    SupplierCode = User.FindFirst("SupplierCode")!.Value,
            //    DocumentType = order.DocumentType!,
            //};
            //// Obtengo los detalles de la Orden de Compra para saber si ya se genero la entrada de mercancia
            //var orderDetails = await _orderService.GetOrderDetailsGoodsReceiptByOrderNumberAsync(orderDetailsDTO);
            //var countDocNum = 0;
            //foreach (var orderDetail in orderDetails)
            //{
            //    if (orderDetail.DocNum != 0)
            //    {
            //        countDocNum++;
            //    }
            //}

            //if (countDocNum == 0)
            //{
            //    ViewBag.Message = $"La orden de compra {model.OrderNumber} no tiene una entrada de mercancía generada.";
            //    return View(model);
            //}

            //var pdfFile = model.FilePDF!;
            //var xmlFile = model.FileXML!;
            //var ordenCompra = model.OrderNumber!;

            //// Validar el tipo de archivo
            //if (pdfFile.ContentType != "application/pdf")
            //{
            //    ModelState.AddModelError("FilePDF", "El archivo debe ser un PDF.");
            //    ViewBag.OrderNumber = model.OrderNumber;
            //    //ViewBag.UserIpAddress = ipAddress;
            //    return View(model);
            //}

            //if (xmlFile.ContentType != "text/xml" && xmlFile.ContentType != "application/xml")
            //{
            //    ModelState.AddModelError("FileXML", "El archivo debe ser un XML.");
            //    ViewBag.OrderNumber = model.OrderNumber;
            //    //ViewBag.UserIpAddress = ipAddress;
            //    return View(model);
            //}

            //// Validar el tamaño del archivo
            //if (pdfFile.Length > 2 * 1024 * 1024)
            //{
            //    ModelState.AddModelError("FilePDF", "El archivo PDF no debe exceder los 2 MB.");
            //    ViewBag.OrderNumber = model.OrderNumber;
            //    //ViewBag.UserIpAddress = ipAddress;
            //    return View(model);
            //}

            //if (xmlFile.Length > 2 * 1024 * 1024)
            //{
            //    ModelState.AddModelError("FileXML", "El archivo XML no debe exceder los 2 MB.");
            //    ViewBag.OrderNumber = model.OrderNumber;
            //    //ViewBag.UserIpAddress = ipAddress;
            //    return View(model);
            //}

            //// Validar el contenido del archivo XML
            //var xmlContent = string.Empty;
            //try
            //{
            //    using (var xmlStream = xmlFile.OpenReadStream())
            //    {
            //        var xmlDoc = new System.Xml.XmlDocument();
            //        xmlDoc.Load(xmlStream);
            //        xmlContent = xmlDoc.InnerXml;
            //    }
            //}
            //catch
            //{
            //    ModelState.AddModelError("FileXML", "El archivo XML no es válido.");
            //    ViewBag.OrderNumber = model.OrderNumber;
            //    //ViewBag.UserIpAddress = ipAddress;
            //    return View(model);
            //}

            //try
            //{
            //    // Leer el contenido del archivo XML
            //    var rfcReceptor = string.Empty;
            //    var invoice = string.Empty;
            //    var serie = string.Empty;
            //    var datos = _filesRepository.GetDataFromXml(xmlContent);
            //    foreach (var dato in datos)
            //    {
            //        dato.SupplierCode = supplierCode;
            //        rfcReceptor = dato.ReceptorRfc;
            //        invoice = dato.Folio;
            //        serie = dato.Serie;
            //    }
            //    // Valida que la factura corresponda a la orden de compra
            //    var countInvoice = 0;
            //    foreach (var orderDetail in orderDetails)
            //    {
            //        if (orderDetail.InvoiceSupplier == invoice)
            //        {
            //            countInvoice++;
            //        }
            //    }
            //    if (countInvoice == 0)
            //    {
            //        ViewBag.Message = $"La factura {serie}{invoice} no corresponde a la registrada en la Entrada de Mercancia, comunicarse con personal de compras.";
            //        return View(model);
            //    }

            //    // Validar el RFC del receptor
            //    if (!(rfcReceptor.Equals("DAM960116H65") || rfcReceptor.Equals("CIN041008173")))
            //    {
            //        ViewBag.Message = $"La factura {serie}{invoice}, no es para LUBER Lubricantes.";
            //        return View(model);
            //    }
            //    //Valida que no se haya cargado la factura anteriormente
            //    var facturaUnica = await _filesRepository.SearchInvoiceAsync(datos[0].UUID);
            //    if (facturaUnica)
            //    {
            //        ViewBag.Message = $"La factura {serie}{invoice}, ya ha sido cargada.";
            //        ViewBag.OrderNumber = model.OrderNumber;
            //        //ViewBag.UserIpAddress = ipAddress;
            //        return View(model);
            //    }

            //    var cDFIStatusModel = new CFDIStatusModel
            //    {
            //        EmisorRFC = datos[0].EmisorRfc,
            //        ReceptorRFC = datos[0].ReceptorRfc,
            //        Total = datos[0].Total.ToString(),
            //        UUID = datos[0].UUID,
            //        Sello = datos[0].Sello
            //    };

            //    var estadoCFDI = await _filesRepository.GetCFDIStatusAsync(cDFIStatusModel);
            //    var estado = string.Empty;
            //    foreach (var xml in estadoCFDI)
            //    {
            //        //estado = xml.Estado!.ToString();
            //        estado = "Vigente";
            //    }
            //    if (estado != "Vigente")
            //    {
            //        ViewBag.Message = $"El CFDI {serie}{invoice} no está vigente. Estado: {estado}.";
            //        //ViewBag.OrderNumber = model.OrderNumber;
            //        //ViewBag.UserIpAddress = ipAddress;
            //        return View(model);
            //    }

            //    // TO DO Separate saving of the invoice to the new view
            //    // Guardar los datos del XML en la base de datos
            //    var result = await _filesRepository.SaveXmlDataInDatabaseAsync(datos, ordenCompra, supplierName!, idUsuario!, ipAddress);

            //    if (result != "OK")
            //    {
            //        ViewBag.Message = $"{result} {ordenCompra}.";
            //        return View(model);
            //    }

            //    string folderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedFiles");
            //    if (!Directory.Exists(folderPath))
            //    {
            //        Directory.CreateDirectory(folderPath);
            //    }

            //    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            //    string pdfFileName = $"{timestamp}_{ordenCompra}_{Path.GetFileName(pdfFile.FileName)}";
            //    string xmlFileName = $"{timestamp}_{ordenCompra}_{Path.GetFileName(xmlFile.FileName)}";
            //    string pdfFilePath = Path.Combine(folderPath, pdfFileName);
            //    string xmlFilePath = Path.Combine(folderPath, xmlFileName);

            //    using (var pdfStream = new FileStream(pdfFilePath, FileMode.Create))
            //    {
            //        pdfFile.CopyTo(pdfStream);
            //    }

            //    using (var xmlStream = new FileStream(xmlFilePath, FileMode.Create))
            //    {
            //        xmlFile.CopyTo(xmlStream);
            //    }

            //    var pdfName = Path.GetFileNameWithoutExtension(Path.GetFileName(pdfFile.FileName));
            //    var xmlName = Path.GetFileNameWithoutExtension(Path.GetFileName(xmlFile.FileName));
            //    var xmlConverted = _filesRepository.ConvertXmlToPdf(xmlContent, Path.Combine(folderPath, $"{timestamp}_1_{xmlName}.pdf"));
            //    var archivos = new List<FileModel>
            //                {
            //                    new() { OrderNumber = model.OrderNumber, Name = pdfName, Route = folderPath, DateTime = timestamp, Extension = ".pdf", Converted = false },
            //                    new() { OrderNumber = model.OrderNumber, Name = xmlName, Route = folderPath, DateTime = timestamp, Extension = ".xml", Converted = false },
            //                    new() { OrderNumber = model.OrderNumber, Name = xmlName, Route = folderPath, DateTime = timestamp, Extension = ".pdf", Converted = true }
            //                };
            //    await _filesRepository.SaveFilesToDatabaseAsync(archivos);

            //    // Enviar correo de confirmación
            //    var userEmail = GetClaimValue(ClaimTypes.Email)!;
            //    var name = GetClaimValue(ClaimTypes.Name)!;
            //    var email = new EmailModel
            //    {
            //        Para = userEmail,
            //        CCO = "programador1@luberoil.com",
            //        Asunto = "Documentos guardados correctamente",
            //        Contenido = $"Hola, {name}<br><br>Las facturas: <br><br> {pdfFile.FileName} y <br> {xmlFile.FileName}. <br><br> Para la orden de compra {ordenCompra} de la Empresa {supplierName} se han guardado correctamente.<br><br>Saludos,<br>El equipo de LUBER Lubricantes"
            //    };
            //    EmailRepository.EnviarCorreo(email, name);

            //    // Redirigir a la lista de órdenes
            //    return RedirectToAction("Index", "Lecturaxml", new { xmlContent });
            //    //return RedirectToAction("ListOrders", "Orders");
            //}
            //catch (Exception ex)
            //{
            //    ViewBag.Message = $"Ocurrió un error al procesar los archivos: {ex.Message}";
            //}
            ////ViewBag.UserIpAddress = ipAddress;
            //ViewBag.OrderNumber = model.OrderNumber;
            //return View(model);
        }

        [HttpPost("UploadFile")]
        public IActionResult UploadFile(string orderNumber, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/UploadedFiles", file.FileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(stream);
                // Lógica adicional para asociar el archivo con la orden
            }
            return RedirectToAction("Details", new { orderNumber });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetDocumentsAsync(int orderNumber)
        {
            var documents = await _filesRepository.GetDocumentsAsync(orderNumber);
            if (documents != null)
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

        private string? GetClaimValue(string claimType)
        {
            return User.FindFirst(claimType)?.Value;
        }

        private ViewResult? ValidateFiles(LoadFileModel model)
        {
            if (model.FilePDF.ContentType != "application/pdf")
            {
                ModelState.AddModelError("FilePDF", "El archivo debe ser un PDF.");
                return View(model);
            }

            if (model.FileXML.ContentType != "text/xml" && model.FileXML.ContentType != "application/xml")
            {
                ModelState.AddModelError("FileXML", "El archivo debe ser un XML.");
                return View(model);
            }

            if (model.FilePDF.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("FilePDF", "El archivo PDF no debe exceder los 2 MB.");
                return View(model);
            }

            if (model.FileXML.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("FileXML", "El archivo XML no debe exceder los 2 MB.");
                return View(model);
            }

            return null;
        }

        private async Task<List<DetailsGoodsReceiptModel>> GetOrderDetailsAsync(OrderDetailDTO orderDetailDTO)
        {
            var result = await GetOrderDetails2Async(orderDetailDTO);

            orderDetailDTO.DocumentType = result.DocumentType;

            return await _orderRepository.GetOrderDetailsGoodsReceiptByOrderNumberAsync(orderDetailDTO);
        }

        private async Task<OrderModel> GetOrderDetails2Async(OrderDetailDTO orderDetailDTO) 
        {
            orderDetailDTO = new OrderDetailDTO
            {
                Action = 2,
                OrderNumber = orderDetailDTO.OrderNumber,
                SupplierCode = orderDetailDTO.SupplierCode,
                DocumentType = orderDetailDTO.DocumentType,
            };

            return await _orderRepository.GetOrderByOrderNumberAsync(orderDetailDTO);
        }

        private static string? ReadXmlContent(IFormFile xmlFile)
        {
            try
            {
                using var xmlStream = xmlFile.OpenReadStream();
                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.Load(xmlStream);
                return xmlDoc.InnerXml;
            }
            catch
            {
                return null;
            }
        }

        private string? ValidateXmlData(List<LecturaXmlModel> datos, string supplierCode)
        {
            foreach (var dato in datos)
            {
                dato.SupplierCode = supplierCode;

                if (!dato.ReceptorRfc.Equals("CIN041008173"))
                {
                    return "La factura no es para LUBER Lubricantes.";
                }

                var facturaUnica = _filesRepository.SearchInvoiceAsync(dato.UUID).Result;
                if (facturaUnica)
                {
                    return "La factura ya ha sido cargada.";
                }
            }

            return null;
        }

        private async Task<string?> ValidateXmlData(List<LecturaXmlModel> datos, string supplierCode, List<DetailsGoodsReceiptModel> orderDetails)
        {
            foreach (var dato in datos)
            {
                dato.SupplierCode = supplierCode;

                if (!orderDetails.Any(od => od.InvoiceSupplier == dato.Folio))
                {
                    return $"La factura {dato.Serie}{dato.Folio} no corresponde a la registrada en la Entrada de Mercancía.";
                }

                if (!(dato.ReceptorRfc.Equals("DAM960116H65") || dato.ReceptorRfc.Equals("CIN041008173")))
                {
                    return $"La factura {dato.Serie}{dato.Folio} no es para LUBER Lubricantes.";
                }
                try
                {
                    var facturaUnica = await _filesRepository.SearchInvoiceAsync(dato.UUID);
                    if (facturaUnica)
                    {
                        return $"La factura {dato.Serie}{dato.Folio} ya ha sido cargada.";
                    }
                }
                catch(Exception ex)
                {
                    return $"Error al validar la factura {dato.Serie}{dato.Folio}: {ex.Message}";
                }
            }

            return null;
        }

        private async Task<string> ValidateCFDIStatus(LecturaXmlModel dato)
        {
            var cFDIStatusModel = new CFDIStatusModel
            {
                EmisorRFC = dato.EmisorRfc,
                ReceptorRFC = dato.ReceptorRfc,
                Total = dato.Total.ToString(),
                UUID = dato.UUID,
                Sello = dato.Sello
            };

            var estadoCFDI = await _filesRepository.GetCFDIStatusAsync(cFDIStatusModel);
            return estadoCFDI.FirstOrDefault()?.Estado ?? "No disponible";
        }

        private async Task SaveFilesToSystem(LoadFileModel model, string xmlContent)
        {
            string folderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedFiles");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string pdfFileName = $"{timestamp}_{model.OrderNumber}_{Path.GetFileName(model.FilePDF.FileName)}";
            string xmlFileName = $"{timestamp}_{model.OrderNumber}_{Path.GetFileName(model.FileXML.FileName)}";
            string pdfFilePath = Path.Combine(folderPath, pdfFileName);
            string xmlFilePath = Path.Combine(folderPath, xmlFileName);

            using (var pdfStream = new FileStream(pdfFilePath, FileMode.Create))
            {
                model.FilePDF.CopyTo(pdfStream);
            }

            using (var xmlStream = new FileStream(xmlFilePath, FileMode.Create))
            {
                model.FileXML.CopyTo(xmlStream);
            }

            var pdfName = Path.GetFileNameWithoutExtension(model.FilePDF.FileName);
            var xmlName = Path.GetFileNameWithoutExtension(model.FileXML.FileName);
            _filesRepository.ConvertXmlToPdf(xmlContent, Path.Combine(folderPath, $"{timestamp}_1_{xmlName}.pdf"));
            var archivos = new List<FileModel>
                            {
                                new() { OrderNumber = model.OrderNumber, Name = pdfName, Route = folderPath, DateTime = timestamp, Extension = ".pdf", Converted = false },
                                new() { OrderNumber = model.OrderNumber, Name = xmlName, Route = folderPath, DateTime = timestamp, Extension = ".xml", Converted = false },
                                new() { OrderNumber = model.OrderNumber, Name = xmlName, Route = folderPath, DateTime = timestamp, Extension = ".pdf", Converted = true }
                            };
            await _filesRepository.SaveFilesToDatabaseAsync(archivos);
        }

        private bool SendConfirmationEmail(LoadFileModel model, string supplierName)
        {
            var userEmail = GetClaimValue(ClaimTypes.Email)!;
            var name = GetClaimValue(ClaimTypes.Name)!;

            var email = new EmailModel
            {
                Para = userEmail,
                CCO = "programador1@luberoil.com; moeazael77@hotmail.com",
                Asunto = "Documentos guardados correctamente",
                Contenido = $"Hola, {name}<br><br>Las facturas: <br><br> {model.FilePDF.FileName} y <br> {model.FileXML.FileName}. <br><br> Para la orden de compra {model.OrderNumber} de la Empresa {supplierName} se han guardado correctamente.<br><br>Saludos,<br>El equipo de LUBER Lubricantes"
            };

            return _emailRepository.SendEmail(email, name);
        }

    }
}