using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Data;
using WebProveedoresN.DTOs;
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
            if (usuario is null)
            {
                throw new ArgumentNullException(nameof(usuario));
            }

            try
            {
                var supplierCode = User.FindFirst("SupplierCode")!.Value;
                if (supplierCode == null)
                {
                    return RedirectToAction("Login", "Access");
                }

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
        public IActionResult InviteUser()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult InviteUser(InviteUserDTO inviteUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(inviteUserDTO);
            }
            var model = new Usuario()
            {
                SupplierCode = User.FindFirst("SupplierCode")?.Value!,
                Nombre = inviteUserDTO.Name,
                Correo = inviteUserDTO.Email,
                Clave = UtilityService.ConvertirSHA256(inviteUserDTO.Password),
                Token = UtilityService.GenerarToken(),
                Restablecer = false,
                Confirmado = false,
                IdStatus = 1,
            };
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
            return View(inviteUserDTO);
        }

        [HttpGet]
        public IActionResult Guardar()
         {
            return View();
        }

        [HttpPost]
        public IActionResult Guardar(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDTO);
            }

            var model = new Usuario()
            {
                SupplierName = TempData["SupplierName"]?.ToString() ?? string.Empty,
                SupplierCode = TempData["SupplierCode"]?.ToString() ?? string.Empty,
                Nombre = registerDTO.Name,
                Correo = registerDTO.Email,
                Clave = registerDTO.Password,
                //Roles = new List<string> { "Proveedor" }
            };
            if (model.Clave != null)
            {
                model.Clave = UtilityService.ConvertirSHA256(model.Clave);
            }
            model.Token = UtilityService.GenerarToken();
            model.Restablecer = false;
            model.Confirmado = false;
            model.IdStatus = 1;

            // Metodo que recibe un objeto de tipo UsuarioModel para guardar en la base de datos
            var respuesta = StartService.SaveUserWithRoles(model); 

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

        [HttpPost("Editar2")]
        public IActionResult Editar2(string token)
        {
            if(string.IsNullOrEmpty(token))
            {
                return BadRequest("Faltan datos necesarios para procesar la solicitud.");
            }
            TempData["Token"] = token;
            try
            {
                return Redirect("/Start/Editar");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error en la redireccion: {ex.Message}");
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Editar()
        {
            var token = TempData["Token"] as string;
            // Metodo solo devuelve la vista
            ViewBag.Status = StatusService.GetStatus() ?? [];
            var usuario = DBStart.GetUserByToken(token!);

            return View(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult Editar(Usuario model)
        {
            ViewBag.Message = null;
            if (!ModelState.IsValid)
            {
                ViewBag.Status = StatusService.GetStatus() ?? [];
                return View();
            }
            var respuesta = DBStart.Editar(model);
            ViewBag.Message = respuesta;
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
                //ViewBag.SupplierName = supplier.Name;
                return View(supplier);
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

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(EmailDTO emailDTO)
        {

            Usuario usuario = DBStart.GetUser(emailDTO);
            //ViewBag.Correo = correo;
            var update = new UpdateDTO()
            {
                Restablecer = true,
                Token = usuario.Token,
                Password = UtilityService.ConvertirSHA256(usuario.Clave)
            };
            if (usuario != null)
            {
                var respuesta = DBStart.ResetPassword(update);

                if (respuesta.Contains("exitosamente"))
                {
                    string path = Path.Combine(_webHostEnvironment.ContentRootPath, "Plantilla", "ResetPassword.html");
                    string content = System.IO.File.ReadAllText(path);
                    string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Start/UpdatePassword?token=" + update.Token);

                    string htmlBody = string.Format(content, usuario.Nombre, url);

                    var correoDTO = new EmalDTO()
                    {
                        Para = emailDTO.Email,
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

        public ActionResult UpdatePassword(string token)
        {
            var updateDto = new UpdateDTO
            {
                Token = token
            };
            return View(updateDto);
        }

        [HttpPost]
        public ActionResult UpdatePassword(UpdateDTO updateDTO)
        {
            if (updateDTO.Password != updateDTO.PasswordConfirm)
            {
                return View(updateDTO);
            }
            updateDTO.Restablecer = false;
            updateDTO.Password = UtilityService.ConvertirSHA256(updateDTO.Password);
            var respuesta = DBStart.UpdatePassword(updateDTO);

            if (respuesta.Contains("exitosamente"))
                ViewBag.Restablecido = false;
            else
                ViewBag.Message = "No se pudo actualizar";

            return View();
        }
    }
}