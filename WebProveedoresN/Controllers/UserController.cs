using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebProveedoresN.DTOs;
using WebProveedoresN.Repositories.Implementations;

namespace WebProveedoresN.Controllers
{
    public class UserController : Controller
    {
        private readonly UserRepository _authService;

        public UserController(UserRepository authService)
        {
            _authService = authService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> EditProfileAsync(EmailDTO emailDTO)
        {
            // Lógica para obtener los datos del usuario actual
            var user = await _authService.GetUserAsync(emailDTO);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfileAsync(UserProfileDTO model)
        {
            if (ModelState.IsValid)
            {
                // Lógica para actualizar los datos del usuario
                await _authService.UpdateProfileAsync(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDTO model)
        {
            if (ModelState.IsValid)
            {
                var update = new UpdateDTO
                {
                    Token = User.FindFirst("Token")!.Value,
                    Restablecer = false,
                    Password = model.NewPassword,
                };
                // Lógica para cambiar la contraseña
                await _authService.UpdatePasswordAsync(update);
                return RedirectToAction("Index");
            }
            return View(model);
        }

    }
}
