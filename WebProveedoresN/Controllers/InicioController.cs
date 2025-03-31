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
        public IActionResult Listar(UsuarioDTO usuario)
        {
            try
            {
                var supplierId = User.FindFirst("SupplierId")?.Value;
                if (supplierId == null)
                {
                    return RedirectToAction("Login", "Access");
                }
                ViewBag.SupplierId = supplierId;

                // Metodo que devuelve una lista de usuarios
                var usuarios = DBInicio.ListarUsuariosConRoles(int.Parse(supplierId));
                return View(usuarios);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar la lista de usuarios: " + ex.Message;
                return View(new List<UsuarioDTO>());
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult InvitarUsuario()
        {
            var supplierName = User.FindFirst("SupplierName")?.Value;
            ViewBag.Empresa = supplierName;
            var usuarioDTO = new UsuarioDTO()
            {
                Empresa = ViewBag.Empresa
            };
            return View(usuarioDTO);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult InvitarUsuario(UsuarioDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (model.Clave != null)
            {
                model.Clave = UtilityService.ConvertirSHA256(model.Clave);
            }
            model.Token = UtilityService.GenerarToken();
            model.Restablecer = false;
            model.Confirmado = false;
            model.IdStatus = 1;
            var respuesta = DBInicio.GuardarInvitadoConRoles(model);
            TempData["Message"] = respuesta;
            if (respuesta.Contains("duplicate"))
            {
                respuesta = $"Ya esta registrado el correo {model.Correo}";
                TempData["Message"] = respuesta;
                return View(model);
            }
            if (respuesta.Contains("exitosamente"))
            {
                string path = Path.Combine(_webHostEnvironment.ContentRootPath, "Plantilla", "Invitacion.html");
                string content = System.IO.File.ReadAllText(path);
                string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Inicio/Confirmar?token=" + model.Token);
                string htmlBody = string.Format(content, model.Nombre, url);
                var correoDTO = new CorreoDTO()
                {
                    Para = model.Correo,
                    Asunto = "Invitación",
                    Contenido = htmlBody
                };
                CorreoServicio.EnviarCorreo(correoDTO, model.Nombre);
                ViewBag.Creado = true;
                respuesta = $"Se ha enviado una invitación al correo {model.Correo}";
                TempData["Message"] = respuesta;
                return RedirectToAction("Listar");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Guardar()
        {
            ViewBag.Empresa = TempData["Empresa"]?.ToString();
            var usuarioDTO = new UsuarioDTO()
            {
                Empresa = ViewBag.Empresa
            };
            //ViewBag.Status = DBStatus.ObtenerEstatus() ?? [];
            ViewBag.Roles = DBRoles.ObtenerRoles() ?? [];
            return View(usuarioDTO);
        }

        [HttpPost]
        public IActionResult Guardar(UsuarioDTO model)
        {
            if (!ModelState.IsValid)
            {
                //ViewBag.Status = DBStatus.ObtenerEstatus() ?? [];
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
            model.IdStatus = 1;

            // Metodo que recibe un objeto de tipo UsuarioModel para guardar en la base de datos
            var respuesta = DBInicio.GuardarUsuarioConRoles(model); ;
            //var respuesta = DBUsuario.Guardar(model);

            TempData["Mensaje"] = respuesta;
            if (respuesta.Contains("exitosamente"))
            {
                string path =Path.Combine(_webHostEnvironment.ContentRootPath,"Plantilla","Confirmar.html");
                string content = System.IO.File.ReadAllText(path);
                string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Inicio/Confirmar?token=" + model.Token);

                string htmlBody = string.Format(content, model.Nombre, url);

                var correoDTO = new CorreoDTO()
                {
                    Para = model.Correo,
                    Asunto = "Correo confirmacion",
                    Contenido = htmlBody
                };

                CorreoServicio.EnviarCorreo(correoDTO, model.Nombre);
                ViewBag.Creado = true;
                ViewBag.Mensaje = $"Su cuenta ha sido creada. Hemos enviado un mensaje al correo {model.Correo} para confirmar su cuenta";

                return RedirectToAction("Listar");
            }
            return RedirectToAction("Guardar");
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Editar(string token)
        {
            // Metodo solo devuelve la vista
            ViewBag.Status = DBStatus.ObtenerEstatus() ?? [];
            var usuario = DBInicio.ObtenerUsuarioConToken(token);

            return View(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult Editar(UsuarioDTO model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Status = DBStatus.ObtenerEstatus() ?? [];
                return View();
            }
            var respuesta = DBInicio.Editar(model);
            TempData["Message"] = respuesta;
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
            if(string.IsNullOrEmpty(empresaDTO.Code))
            {
                ViewBag.Empresa = empresaDTO.Empresa;
                return View();
            }
            // TODO: que valide si ya existe el proveedor con el codigo de proveedor
            var respuesta = Convert.ToBoolean(DBInicio.ValidarPrimerUsuario(empresaDTO));

            if (respuesta)
            {
                TempData["Mensaje"] = $"Ya existe un Administrador para la empresa {empresaDTO.Empresa}, solicite se le envíe una Invitación.";
                return View(empresaDTO);
            }
            TempData["Empresa"] = empresaDTO.Empresa;
            return RedirectToAction("Guardar");


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

                    CorreoServicio.EnviarCorreo(correoDTO, usuario.Nombre);
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
