using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Repositories.Implementations;

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

            var archivos = XmlRepository.GetDataFromXml(xmlContent);
            return View(archivos);
        }
    }
}