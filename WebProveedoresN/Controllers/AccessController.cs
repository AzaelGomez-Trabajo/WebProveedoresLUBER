using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebProveedoresN.DTOs;
using WebProveedoresN.Repositories.Interfaces;
using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class AccessController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AccessController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            ViewBag.Message = null;
            if (!ModelState.IsValid)
            {
                return View(loginDTO);
            }
            var user = await _userRepository.ValidateUserAsync(loginDTO.Email, UtilityService.ConvertirSHA256(loginDTO.Password));
            if (user != null && user.FullName != null)
            {
                if (!user.Confirmado)
                {
                    ViewBag.Message = $"Falta confirmar su cuenta, favor revise su bandeja del correo {user.Email}.";
                }
                else if (user.Restablecer)
                {
                    ViewBag.Message = $"Se ha solicitado restablecer su cuenta, favor revise su bandeja del correo {user.Email}.";
                }
                else if (user.IdStatus != 1)
                {
                    ViewBag.Message = $"Cuenta bloqueada, comuniquese con su administrador.";
                }
                else
                {
                    // crear las claims para el usuario con las cookies
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, user.FullName),
                        new(ClaimTypes.Email, user.Email),
                        new("SupplierCode", user.SupplierCode),
                        new("SupplierName", user.SupplierName),
                        new("IdUsuario", user.IdUsuario.ToString()),
                        new("Token", user.Token.ToString()),
                    };

                    // agregar los roles del usuario
                    foreach (var rol in user.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rol));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    // termina la autenticación

                    return RedirectToAction("ListOrders", "Orders");
                }
            }
            else
            {
                ViewBag.Message = "Correo o contraseña incorrectos";
            }
            return View();
        }

        [Authorize]
        public IActionResult VerRol()
        {
            var roles = User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

            ViewBag.Roles = roles;
            return View();
        }

        public async Task<IActionResult> Salir()
        {
            // elimiar la cookie de autenticación
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Access");
        }
    }
}