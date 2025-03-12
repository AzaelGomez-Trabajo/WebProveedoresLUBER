using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Data;
using WebProveedoresN.Models;
using WebProveedoresN.Services;
using WebProveedoresN.Conexion;

namespace WebProveedoresN.Controllers
{
    public class UsuarioController : Controller
    {
        // Metodo que devuelve la vista con la lista de usuarios
        public IActionResult Listar()
        {

            var usuarios = DBUsuario.ListarUsuariosConRoles();
            return View(usuarios);
            /*
            // Instanciar la clase DBUsuario
            var lista = DBUsuario.Listar();
            return View(lista);
            */
        }

        public IActionResult Guardar()
        {
            // Metodo solo devuelve la vista
            ViewBag.Status = DBStatus.ObtenerEstatus();
            ViewBag.Roles = DBUsuario.ObtenerRoles();
            return View();
        }

        // Metodo para guardar un usuario
        [HttpPost]
        public IActionResult Guardar(UsuarioModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Status = DBStatus.ObtenerEstatus();
                ViewBag.Roles = DBUsuario.ObtenerRoles();
                return View(model);
            }

            if (model.Clave != null)
            {
                model.Clave = UtilityService.ConvertirSHA256(model.Clave);
            }
            model.Token = UtilityService.GenerarToken();
            model.Restablecer = false;
            model.Confirmado = false;

            // Metodo que recibe un objeto de tipo UsuarioModel para guardar en la base de datos

            var respuesta = DBUsuario.GuardarUsuarioConRoles(model); ;
            //var respuesta = DBUsuario.Guardar(model);

            TempData["Mensaje"] = respuesta;
            if (respuesta.Contains("exitosamente"))
            {
                return RedirectToAction("Listar");
            }
            return RedirectToAction("Guardar");
        }

        public IActionResult Editar(string token)
        {
            // Metodo solo devuelve la vista
            ViewBag.Status = DBStatus.ObtenerEstatus() ?? [];
            ViewBag.Roles = DBUsuario.ObtenerRoles() ?? [];
            var usuario = DBUsuario.ObtenerUsuario(token);
            return View(usuario);
        }

        [HttpPost]
        public IActionResult Editar(UsuarioModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Status = DBStatus.ObtenerEstatus();
                ViewBag.Roles = DBUsuario.ObtenerRoles();
                return View();
            }
            var respuesta = DBUsuario.EditarUsuarioConRoles(model);
            //var respuesta = DBUsuario.Editar(model);

            TempData["Mensaje"] = respuesta;
            if (respuesta.Contains("exitosamente"))
            {
                return RedirectToAction("Listar");
            }
            else
            {
                return View(model);
            }
        }
    }
}
