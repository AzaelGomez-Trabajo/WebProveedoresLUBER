using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebProveedoresN.Data;
using WebProveedoresN.Models;
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
        public async Task<IActionResult> Login(UsuarioDTO model)
        {
            var usuario = DBInicio.ValidarUsuario(model.Correo, UtilityService.ConvertirSHA256(model.Clave));
            if (usuario != null)
            {
                // crear las claims para el usuario con las cookies
                var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, usuario.Nombre),
                        new(ClaimTypes.Email, usuario.Correo),
                    };

                foreach (var rol in usuario.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                // termina la autenticación

                return RedirectToAction("Index", "Orders", new { empresa = usuario.Empresa });
            }
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
