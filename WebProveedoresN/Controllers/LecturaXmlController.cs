using Microsoft.AspNetCore.Mvc;

using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class LecturaXmlController : Controller
    {
        public ActionResult Index(string xmlFilePath)
        {
            if (string.IsNullOrEmpty(xmlFilePath))
            {
                ViewBag.Message = "La ruta del archivo XML no es válida.";
                return View();
            }

            var archivos = XmlServicio.ObtenerDatosDesdeXml(xmlFilePath);
            return View(archivos);

        }
    }
}