using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Services;
using System.Security.Claims;
using ProyectoCanvas.Services.Utilities;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Text;

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

            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado.", field = "correo" });
            }

            if (!VerifyPlainPassword(password, usuario.PasswordHash))
            {
                return Json(new { success = false, message = "Contraseña incorrecta.", field = "password" });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Correo),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties();

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

        private bool VerifyPlainPassword(string password, byte[] storedPassword)
        {
            // Convertir storedPassword de VARBINARY a string para comparar en texto plano
            var storedPasswordString = System.Text.Encoding.UTF8.GetString(storedPassword);

            return password == storedPasswordString;
        }
    }
}
