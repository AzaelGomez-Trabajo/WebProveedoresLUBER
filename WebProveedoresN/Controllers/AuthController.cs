using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebProveedoresN.Data;
using WebProveedoresN.DTOs;
using WebProveedoresN.Models;
using WebProveedoresN.Repositories.Implementations;
using WebProveedoresN.Repositories.Interfaces;
using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class AuthController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserRepository _userRepository;

        public AuthController(IWebHostEnvironment webHostEnvironment, IUserRepository userRepository)
        {
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index(UserModel userModel)
        {
            if (userModel is null)
            {
                throw new ArgumentNullException(nameof(userModel));
            }

            try
            {
                var supplierCode = User.FindFirst("SupplierCode")!.Value;
                if (supplierCode == null)
                {
                    return RedirectToAction("Login", "Access");
                }

                // Metodo que devuelve una lista de usuarios
                var users = await _userRepository.ListarUsuariosConRolesAsync(supplierCode);
                return View(users);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar la lista de usuarios: " + ex.Message;
                return View(new List<UserModel>());
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult InviteUser()
        {
            return View();
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> InviteUser(InviteUserDTO inviteUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(inviteUserDTO);
            }
            var model = new UserModel()
            {
                SupplierCode = User.FindFirst("SupplierCode")?.Value!,
                FullName = inviteUserDTO.Name,
                Email = inviteUserDTO.Email,
                Password = UtilityService.ConvertirSHA256(inviteUserDTO.Password),
                Token = UtilityService.GenerarToken(),
                Restablecer = false,
                Confirmado = false,
                IdStatus = 1,
            };
            var answer = await _userRepository.SaveGuestWithRolesAsync(model);
            TempData["Message"] = answer;
            if (answer.Contains("duplicate"))
            {
                answer = $"Ya esta registrado el correo {model.Email}";
                TempData["Message"] = answer;
                return View(model);
            }
            if (answer.Contains("exitosamente"))
            {
                string path = Path.Combine(_webHostEnvironment.ContentRootPath, "Plantilla", "Invitacion.html");
                string content = System.IO.File.ReadAllText(path);
                string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Auth/ConfirmEmailAsync?token=" + model.Token);
                string htmlBody = string.Format(content, model.FullName, url);
                var correoDTO = new EmailModel()
                {
                    Para = model.Email,
                    Asunto = "Invitación",
                    Contenido = htmlBody
                };
                EmailRepository.EnviarCorreo(correoDTO, model.FullName);
                ViewBag.Creado = true;
                answer = $"Se ha enviado una invitación al correo {model.Email}";
                TempData["Message"] = answer;
                return RedirectToAction("Index");
            }
            return View(inviteUserDTO);
        }

        [HttpGet]
        public IActionResult SaveUserAsync()
        {
            return View();
        }

        [HttpPost("SaveUser")]
        public async Task<IActionResult> SaveUserAsync(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDTO);
            }

            var model = new UserModel()
            {
                SupplierName = TempData["SupplierName"]?.ToString() ?? string.Empty,
                SupplierCode = TempData["SupplierCode"]?.ToString() ?? string.Empty,
                FullName = registerDTO.FullName,
                Email = registerDTO.Email,
                Password = registerDTO.Password,
                //Roles = new List<string> { "Proveedor" }
            };
            if (model.Password != null)
            {
                model.Password = UtilityService.ConvertirSHA256(model.Password);
            }
            model.Token = UtilityService.GenerarToken();
            model.Restablecer = false;
            model.Confirmado = false;
            model.IdStatus = 1;

            // Metodo que recibe un objeto de tipo UsuarioModel para guardar en la base de datos
            var respuesta = await _userRepository.SaveUserWithRolesAsync(model);

            TempData["Message"] = respuesta;
            if (respuesta.Contains("exitosamente"))
            {
                string path = Path.Combine(_webHostEnvironment.ContentRootPath, "Plantilla", "Confirmar.html");
                string content = System.IO.File.ReadAllText(path);
                string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Auth/ConfirmEmailAsync?token=" + model.Token);

                string htmlBody = string.Format(content, model.FullName, url);

                var correoDTO = new EmailModel()
                {
                    Para = model.Email,
                    Asunto = "Correo confirmacion",
                    Contenido = htmlBody
                };

                EmailRepository.EnviarCorreo(correoDTO, model.FullName);
                ViewBag.Creado = true;
                ViewBag.Mensaje = $"Su cuenta ha sido creada. Hemos enviado un mensaje al correo {model.Email} para confirmar su cuenta";

                return RedirectToAction("Listar");
            }
            return RedirectToAction("Guardar");
        }

        [HttpPost("Editar2")]
        public IActionResult Editar2(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Faltan datos necesarios para procesar la solicitud.");
            }
            TempData["Token"] = token;
            try
            {
                return Redirect("/Auth/Editar");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error en la redireccion: {ex.Message}");
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> EditarAsync()
        {
            var token = TempData["Token"] as string;
            // Metodo solo devuelve la vista
            ViewBag.Status = StatusRepository.GetStatus() ?? [];
            var user = await _userRepository.GetUserByTokenAsync(token!);
            var updateUserDTO = new UpdateUserDTO
            {
                FullName = user.FullName,
                Email = user.Email,
                StatusId = user.IdStatus,
                Token = user.Token,
            };

            return View(updateUserDTO);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("Editar")]
        public async Task<IActionResult> EditarAsync(UpdateUserDTO model)
        {
            ViewBag.Message = null;
            if (!ModelState.IsValid)
            {
                ViewBag.Status = StatusRepository.GetStatus() ?? [];
                return View();
            }
            var answer = await _userRepository.UpdateUserAsync(model);
            ViewBag.Message = answer;
            if (answer.Contains("exitosamente"))
            {
                return RedirectToAction("Index");
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

        [HttpPost("Register")]
        public async Task<ActionResult> RegisterAsync(SupplierModel supplier)
        {
            bool answer;
            if (string.IsNullOrEmpty(supplier.Code))
            {
                //ViewBag.SupplierName = supplier.Name;
                return View(supplier);
            }

            answer = await _userRepository.ValidateSupplierAsync(supplier);

            if (!answer)
            {
                TempData["Message"] = $"No coincide la informacion proporcionada en los registros";
                return View(supplier);
            }

            answer = await _userRepository.ValidateFirstUserAsync(supplier);

            if (answer)
            {
                TempData["Message"] = $"Ya existe un Administrador para la empresa {supplier.Name}, solicite se le envíe una Invitación.";
                return View(supplier);
            }
            TempData["SupplierName"] = supplier.Name;
            TempData["SupplierCode"] = supplier.Code;

            return RedirectToAction("Guardar");
        }

        public async Task<ActionResult> ConfirmEmailAsync(string token)
        {
            ViewBag.Respuesta = await _userRepository.ConfirmEmailAsync(token);
            return View();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult> ResetPasswordAsync(EmailDTO emailDTO)
        {
            var user = await _userRepository.GetUserAsync(emailDTO);
            //ViewBag.Correo = correo;
            var update = new UpdateDTO()
            {
                Restablecer = true,
                Token = user.Token,
                Password = UtilityService.ConvertirSHA256(user.Password)
            };
            if (user != null)
            {
                var respuesta = await _userRepository.ResetPasswordAsync(update);

                if (respuesta.Contains("exitosamente"))
                {
                    string path = Path.Combine(_webHostEnvironment.ContentRootPath, "Plantilla", "ResetPassword.html");
                    string content = System.IO.File.ReadAllText(path);
                    string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, "/Auth/UpdatePassword?token=" + update.Token);

                    string htmlBody = string.Format(content, user.FullName, url);

                    var correoDTO = new EmailModel()
                    {
                        Para = emailDTO.Email,
                        Asunto = "Restablecer cuenta",
                        Contenido = htmlBody
                    };

                    EmailRepository.EnviarCorreo(correoDTO, user.FullName);
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

        [HttpPost("UpdatePassword")]
        public async Task<ActionResult> UpdatePasswordAsync(UpdateDTO updateDTO)
        {
            if (updateDTO.Password != updateDTO.PasswordConfirm)
            {
                return View(updateDTO);
            }
            updateDTO.Restablecer = false;
            updateDTO.Password = UtilityService.ConvertirSHA256(updateDTO.Password);
            var answer = await _userRepository.UpdatePasswordAsync(updateDTO);

            if (answer.Contains("exitosamente"))
                ViewBag.Restablecido = false;
            else
                ViewBag.Message = "No se pudo actualizar";

            return View();
        }
    }
}