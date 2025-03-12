using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebProveedoresN.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Pagina1()
    {
        return View();
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Pagina2()
    {
        return View();
    }

    public IActionResult Pagina3()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
