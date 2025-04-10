using Microsoft.AspNetCore.Mvc;

using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class LecturaXmlController : Controller
    {
        public ActionResult Index(string xmlContent)
        {
            if (string.IsNullOrEmpty(xmlContent))
            {
                ViewBag.Message = "La ruta del archivo XML no es válida.";
                return View();
            }

            var archivos = XmlServicio.GetDataFromXml(xmlContent);
            return View(archivos);
        }
    }
}