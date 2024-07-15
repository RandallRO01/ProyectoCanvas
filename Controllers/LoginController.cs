using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Services;

namespace ProyectoCanvas.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IRepositorioUsuarios _repositorioUsuarios;

        public LoginController(ILogger<LoginController> logger, IRepositorioUsuarios repositorioUsuarios)
        {
            _logger = logger;
            _repositorioUsuarios = repositorioUsuarios;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string correo, string password, string role)
        {
            var usuario = await _repositorioUsuarios.ObtenerPorCorreo(correo);

            if (usuario != null && VerifyPasswordHash(password, usuario.PasswordHash))
            {
                // Iniciar sesión del usuario
                // Código para manejar la autenticación
                return RedirectToAction("Dashboard", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(storedHash);
            }
        }
    }
}
