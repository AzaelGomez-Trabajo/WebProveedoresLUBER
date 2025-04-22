using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Data;
using WebProveedoresN.Entities;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class StartController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StartController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Listar(Usuario usuario)
        {
            try
            {
                var supplierCode = User.FindFirst("SupplierCode")?.Value;
                if (supplierCode == null)
                {
                    return RedirectToAction("Login", "Access");
                }
                ViewBag.SupplierCode = supplierCode;

                // Metodo que devuelve una lista de usuarios
                var usuarios = DBStart.ListarUsuariosConRoles(supplierCode);
                return View(usuarios);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar la lista de usuarios: " + ex.Message;
                return View(new List<Usuario>());
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult InvitarUsuario()
        {
            var usuario = new Usuario()
            {
                SupplierName = User.FindFirst("SupplierName")?.Value,
                SupplierCode = User.FindFirst("SupplierCode")?.Value,

            };
            return View(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult InvitarUsuario(Usuario model)
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
            var respuesta = DBStart.SaveGuestWithRoles(model);
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
                string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Start/ConfirmEmail?token=" + model.Token);
                string htmlBody = string.Format(content, model.Nombre, url);
                var correoDTO = new EmalDTO()
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
            ViewBag.SupplierName = TempData["SupplierName"]?.ToString();
            ViewBag.SupplierCode = TempData["SupplierCode"]?.ToString();
            var usuario = new Usuario()
            {
                SupplierName = ViewBag.SupplierName,
                SupplierCode = ViewBag.SupplierCode,
            };
            ViewBag.Status = StatusService.GetStatus() ?? [];
            ViewBag.Roles = RoleService.GetRoles() ?? [];
            return View(usuario);
        }

        [HttpPost]
        public IActionResult Guardar(Usuario model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Status = StatusService.GetStatus() ?? [];
                ViewBag.Roles = RoleService.GetRoles() ?? [];
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
            var respuesta = StartService.SaveUserWithRoles(model); ;

            TempData["Message"] = respuesta;
            if (respuesta.Contains("exitosamente"))
            {
                string path = Path.Combine(_webHostEnvironment.ContentRootPath, "Plantilla", "Confirmar.html");
                string content = System.IO.File.ReadAllText(path);
                string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Start/ConfirmEmail?token=" + model.Token);

                string htmlBody = string.Format(content, model.Nombre, url);

                var correoDTO = new EmalDTO()
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
            ViewBag.Status = StatusService.GetStatus() ?? [];
            var usuario = DBStart.GetUserByToken(token);

            return View(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult Editar(Usuario model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Status = StatusService.GetStatus() ?? [];
                return View();
            }
            var respuesta = DBStart.Editar(model);
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

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Supplier supplier)
        {
            var answer = false;
            if (string.IsNullOrEmpty(supplier.Code))
            {
                ViewBag.SupplierName = supplier.Name;
                return View();
            }

            answer = StartService.ValidateSupplier(supplier);

            if (!answer)
            {
                TempData["Message"] = $"No coincide la informacion proporcionada en los registros";
                return View(supplier);
            }

            answer = StartService.ValidateFirstUser(supplier);

            if (answer)
            {
                TempData["Message"] = $"Ya existe un Administrador para la empresa {supplier.Name}, solicite se le envíe una Invitación.";
                return View(supplier);
            }
            TempData["SupplierName"] = supplier.Name;
            TempData["SupplierCode"] = supplier.Code;
            return RedirectToAction("Guardar");
        }

        public ActionResult ConfirmEmail(string token)
        {
            ViewBag.Respuesta = StartService.ConfirmEmail(token);
            return View();
        }

        public ActionResult Restablecer()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Restablecer(string correo)
        {
            Usuario usuario = DBStart.Obtener(correo);
            ViewBag.Correo = correo;
            if (usuario != null)
            {
                var respuesta = DBStart.RestablecerActualizar(1, usuario.Clave, usuario.Token);

                if (respuesta.Contains("exitosamente"))
                {
                    string path = Path.Combine(_webHostEnvironment.ContentRootPath, "Plantilla", "Restablecer.html");
                    string content = System.IO.File.ReadAllText(path);
                    string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Inicio/Actualizar?token=" + usuario.Token);

                    string htmlBody = string.Format(content, usuario.Nombre, url);

                    var correoDTO = new EmalDTO()
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

            var respuesta = DBStart.RestablecerActualizar(0, UtilityService.ConvertirSHA256(clave), token);

            if (respuesta.Contains("exitosamente"))
                ViewBag.Restablecido = true;
            else
                ViewBag.Message = "No se pudo actualizar";

            return View();
        }
    }
}