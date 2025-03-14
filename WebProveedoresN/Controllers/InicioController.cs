using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Data;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class InicioController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InicioController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Listar()
        {
            var usuarios = DBInicio.ListarUsuariosConRoles();
            return View(usuarios);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Guardar()
        {
            ViewBag.Status = DBStatus.ObtenerEstatus() ?? [];
            ViewBag.Roles = DBRoles.ObtenerRoles() ?? [];
            return View();
        }

        [HttpPost]
        public IActionResult Guardar(UsuarioDTO model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Status = DBStatus.ObtenerEstatus() ?? [];
                ViewBag.Roles = DBRoles.ObtenerRoles() ?? [];
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
            var respuesta = DBInicio.GuardarUsuarioConRoles(model); ;
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
            ViewBag.Roles = DBRoles.ObtenerRoles() ?? [];
            var usuario = DBInicio.ObtenerUsuarioConToken(token);
            return View(usuario);
        }

        [HttpPost]
        public IActionResult Editar(UsuarioDTO model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Status = DBStatus.ObtenerEstatus() ?? [];
                ViewBag.Roles = DBRoles.ObtenerRoles() ?? [];
                return View();
            }
            var respuesta = DBInicio.EditarUsuarioConRoles(model);
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

        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registrar(EmpresaDTO empresaDTO)
        {
            if(empresaDTO.Clave != empresaDTO.ConfirmarClave)
            {
                ViewBag.Empresa = empresaDTO.Empresa;
                ViewBag.Message = "Las contraseñas no coinciden";
                return View();
            }
            ViewBag.Empresa = empresaDTO.Empresa;
            var respuesta = Convert.ToBoolean(DBInicio.ValidarPrimerUsuario(empresaDTO));

            if (respuesta)
            {
                return RedirectToAction("Guardar");
            }
            TempData["Mensaje"] = $"Ya existe un Administrador para la empresa {empresaDTO.Empresa}, solicite se le envíe una Invitación.";
            return View(empresaDTO);
            //return RedirectToAction("Login", "Access");
        }

        public ActionResult Confirmar(string token)
        {
            ViewBag.Respuesta = DBInicio.Confirmar(token);
            return View();
        }

        public ActionResult Restablecer()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Restablecer(string correo)
        {
            UsuarioDTO usuario = DBInicio.Obtener(correo);
            ViewBag.Correo = correo;
            if (usuario != null)
            {
                var respuesta = DBInicio.RestablecerActualizar(1, usuario.Clave, usuario.Token);

                if (respuesta.Contains("exitosamente"))
                {
                    string path = Path.Combine(_webHostEnvironment.ContentRootPath,"Plantilla","Restablecer.html");
                    string content = System.IO.File.ReadAllText(path);
                    string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Inicio/Actualizar?token=" + usuario.Token);

                    string htmlBody = string.Format(content, usuario.Nombre, url);

                    var correoDTO = new CorreoDTO()
                    {
                        Para = correo,
                        Asunto = "Restablecer cuenta",
                        Contenido = htmlBody
                    };

                    CorreoServicio.EnviarCorreo(correoDTO);
                    ViewBag.Restablecido = true;
                }
                else
                {
                    ViewBag.Message = "No se pudo restablecer la cuenta";
                }

            }
            else
            {
                ViewBag.Message = "No se encontraron coincidencias con el correo";
            }

            return View();
        }

        public ActionResult Actualizar(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public ActionResult Actualizar(string token, string clave, string confirmarClave)
        {
            ViewBag.Token = token;
            if (clave != confirmarClave)
            {
                ViewBag.Message = "Las contraseñas no coinciden";
                return View();
            }

            var respuesta = DBInicio.RestablecerActualizar(0, UtilityService.ConvertirSHA256(clave), token);

            if (respuesta.Contains("exitosamente"))
                ViewBag.Restablecido = true;
            else
                ViewBag.Message = "No se pudo actualizar";

            return View();
        }

    }
}
