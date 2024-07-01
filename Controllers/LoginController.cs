using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Data;
using System.Security.Claims;

namespace ProyectoCanvas.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext context;

        public LoginController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string correo, string password, string role)
        {
            // Simple validation example
            if (correo == "admin@ulacit.ed.cr" && password == "admin")
            {
                // Authenticate user
                HttpContext.Session.SetString("Correo", correo);
                HttpContext.Session.SetString("Role", role);

                _logger.LogInformation("User authenticated: {Correo}, Role: {Role}", correo, role);

                return RedirectToAction("Index", "Home");
            }
            _logger.LogWarning("Invalid login attempt for user: {Correo}, Password: {Password}", correo, password);

            // Show error message
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}

