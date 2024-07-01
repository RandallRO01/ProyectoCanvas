using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Data;
using System.Security.Claims;

namespace ProyectoCanvas.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly ApplicationDbContext _context;

        public LoginController(ILogger<LoginController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string correo, string password, string role)
        {
            // Validación simple de ejemplo
            if (correo == "admin@ulacit.ed.cr" && password == "admin")
            {
                // Autenticar usuario
                HttpContext.Session.SetString("Correo", correo);
                HttpContext.Session.SetString("Role", role);

                _logger.LogInformation("User authenticated: {Correo}, Role: {Role}", correo, role);

                // Establecer la cookie de autenticación
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, correo),
                    new Claim(ClaimTypes.Role, role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning("Invalid login attempt for user: {Correo}, Password: {Password}", correo, password);

            // Mostrar mensaje de error
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}
