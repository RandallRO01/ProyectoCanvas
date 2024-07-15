using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Models;
using ProyectoCanvas.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace ProyectoCanvas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepositorioRoles _repositorioRoles;

        public HomeController(ILogger<HomeController> logger, IRepositorioRoles repositorioRoles)
        {
            _logger = logger;
            _repositorioRoles = repositorioRoles;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool esProfesor = await _repositorioRoles.EsUsuarioEnRol(usuarioId, "Profesor");
            ViewBag.EsProfesor = esProfesor;

            var cursos = new List<Cursos>
            {
                new Cursos { NombreCurso = "Course 1", Descripcion = "Description 1", ImagenUrl = "https://th.bing.com/th/id/OIP.ijWNEiPVAtBLrzw6F-yZxgAAAA?rs=1&pid=ImgDetMain" },
                new Cursos { NombreCurso = "Course 2", Descripcion = "Description 2", ImagenUrl = "https://th.bing.com/th/id/OIP.ijWNEiPVAtBLrzw6F-yZxgAAAA?rs=1&pid=ImgDetMain" },
                new Cursos { NombreCurso = "Course 3", Descripcion = "Description 3", ImagenUrl = "https://th.bing.com/th/id/OIP.ijWNEiPVAtBLrzw6F-yZxgAAAA?rs=1&pid=ImgDetMain" },
                new Cursos { NombreCurso = "Course 4", Descripcion = "Description 4", ImagenUrl = "https://th.bing.com/th/id/OIP.ijWNEiPVAtBLrzw6F-yZxgAAAA?rs=1&pid=ImgDetMain" }
            };

            return View(cursos);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
