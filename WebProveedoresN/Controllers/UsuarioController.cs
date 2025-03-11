using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Data;
using WebProveedoresN.Models;
using WebProveedoresN.Services;
using WebProveedoresN.Conexion;

namespace WebProveedoresN.Controllers
{
    public class UsuarioController : Controller
    {
        public IActionResult Listar()
        {
            // Instanciar la clase DBUsuario
            var lista = DBUsuario.Listar();
            return View(lista);
        }

        public IActionResult Guardar()
        {
            // Metodo solo devuelve la vista
            return View();
        }

        [HttpPost]
        public IActionResult Guardar(UsuarioModel usuario)
        {
            if (usuario.Clave != null)
            {
                usuario.Clave = UtilityService.ConvertirSHA256(usuario.Clave);
            }
            usuario.Token = UtilityService.GenerarToken();
            usuario.IdAcceso = 2;
            usuario.IdStatus = 1;
            usuario.Restablecer = false;
            usuario.Confirmado = false;

            // Metodo que recibe un objeto de tipo UsuarioModel para guardar en la base de datos

            var respuesta = DBUsuario.Guardar(usuario);

            TempData["Mensaje"] = respuesta;
            if (respuesta.Contains("exitosamente"))
            {
                return RedirectToAction("Listar");
            }
            else 
            {
                return RedirectToAction("Guardar"); 
            }
        }

    }
}
