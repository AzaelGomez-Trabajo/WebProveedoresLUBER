using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Models;
using WebProveedoresN.Data;

namespace WebProveedoresN.Controllers
{
    public class AccessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(UsuarioModel model)
        {
            var usuario = DBUsuario.ValidarUsuario(model.Correo, model.Clave);
            if(usuario != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}
