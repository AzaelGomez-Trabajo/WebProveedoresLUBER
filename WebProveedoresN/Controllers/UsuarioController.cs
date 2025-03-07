using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Controllers
{
    public class UsuarioController : Controller
    {
        DBUsuario _DBUsuario = new DBUsuario();

        public IActionResult Listar()
        {
            // Instanciar la clase DBUsuario
            var lista = _DBUsuario.Listar();
            return View();
        }

        public IActionResult Guardar()
        {
            // Metodo solo devuelve la vista
            return View();
        }
        [HttpPost]
        public IActionResult Guardar()
        {
            return View();
        }

    }
}
