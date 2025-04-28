using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebProveedoresN.DTOs;
using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class AccessController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var usuario = StartService.ValidateUser(loginDTO.Email, UtilityService.ConvertirSHA256(loginDTO.Password));
            if (usuario != null && usuario.Nombre != null)
            {
                if (!usuario.Confirmado)
                {
                    ViewBag.Message = $"Falta confirmar su cuenta, favor revise su bandeja del correo {usuario.Correo}.";
                }
                else if (usuario.Restablecer)
                {
                    ViewBag.Message = $"Se ha solicitado restablecer su cuenta, favor revise su bandeja del correo {usuario.Correo}.";
                }
                else
                {
                    // crear las claims para el usuario con las cookies
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, usuario.Nombre),
                        new(ClaimTypes.Email, usuario.Correo),
                        new("SupplierCode", usuario.SupplierCode),
                        new("SupplierName", usuario.SupplierName),
                        new("IdUsuario", usuario.IdUsuario.ToString()),
                    };

                    // agregar los roles del usuario
                    foreach (var rol in usuario.Roles)
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