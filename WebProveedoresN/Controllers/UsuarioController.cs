using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Data;
using WebProveedoresN.Models;
using WebProveedoresN.Services;
using WebProveedoresN.Conexion;
using Microsoft.AspNetCore.Authorization;

namespace WebProveedoresN.Controllers
{
    public class UsuarioController : Controller
    {
        [Authorize(Roles = "Administrador")]
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

        [Authorize(Roles = "Administrador")]
        public IActionResult Guardar()
        {
            ViewBag.Status = DBStatus.ObtenerEstatus();
            ViewBag.Roles = DBUsuario.ObtenerRoles();
            return View();
        }

        [HttpPost]
        public IActionResult Guardar(UsuarioDTO model)
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

        [Authorize(Roles = "Administrador")]
        public IActionResult Editar(string token)
        {
            // Metodo solo devuelve la vista
            ViewBag.Status = DBStatus.ObtenerEstatus() ?? [];
            ViewBag.Roles = DBUsuario.ObtenerRoles() ?? [];
            var usuario = DBUsuario.ObtenerUsuario(token);
            return View(usuario);
        }

        [HttpPost]
        public IActionResult Editar(UsuarioDTO model)
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
