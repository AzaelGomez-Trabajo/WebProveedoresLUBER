using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Repositories.Interfaces;

namespace WebProveedoresN.Controllers
{
    public class LecturaXmlController : Controller
    {
        private readonly IFilesRepository _filesRepository;

        public LecturaXmlController(IFilesRepository filesRepository)
        {
            _filesRepository = filesRepository;
        }
        public ActionResult Index(string xmlContent)
        {
            if (string.IsNullOrEmpty(xmlContent))
            {
                ViewBag.Message = "La ruta del archivo XML no es válida.";
                return View();
            }

            var archivos = _filesRepository.GetDataFromXml(xmlContent);
            return View(archivos);
        }
    }
}