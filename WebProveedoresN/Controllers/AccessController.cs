using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Models;
using WebProveedoresN.Data;
using WebProveedoresN.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using NuGet.Packaging;

namespace WebProveedoresN.Controllers
{
    public class AccessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UsuarioModel model)
        {
            var usuario = DBUsuario.ValidarUsuario(model.Correo, UtilityService.ConvertirSHA256(model.Clave));
            if (usuario != null)
            {
                // crear las claims para el usuario con las cookies
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuario.Nombre),
                        new Claim(ClaimTypes.Email, usuario.Correo),
                    };

                foreach (var rol in usuario.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                // termina la autenticación

                return RedirectToAction("Listar", "Usuario");
            }
            return View();
        }

        public async Task<IActionResult> Salir()
        {
            // elimiar la cookie de autenticación
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Access");
        }
    }
}
