using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Data;
using ProyectoCanvas.Models;
using System.Diagnostics;

namespace ProyectoCanvas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public IActionResult Index()
        {
            var cursos = new List<Cursos>
            {
                new Cursos { NombreCurso = "Course 1", Descripcion = "Description 1", ImagenUrl = "https://th.bing.com/th/id/OIP.ijWNEiPVAtBLrzw6F-yZxgAAAA?rs=1&pid=ImgDetMain", Link = "/course1" },
                new Cursos { NombreCurso = "Course 2", Descripcion = "Description 2", ImagenUrl = "https://th.bing.com/th/id/OIP.ijWNEiPVAtBLrzw6F-yZxgAAAA?rs=1&pid=ImgDetMain", Link = "/course2" },
                new Cursos { NombreCurso = "Course 3", Descripcion = "Description 3", ImagenUrl = "https://th.bing.com/th/id/OIP.ijWNEiPVAtBLrzw6F-yZxgAAAA?rs=1&pid=ImgDetMain", Link = "/course3" },
                new Cursos { NombreCurso = "Course 4", Descripcion = "Description 4", ImagenUrl = "https://th.bing.com/th/id/OIP.ijWNEiPVAtBLrzw6F-yZxgAAAA?rs=1&pid=ImgDetMain", Link = "/course4" }
            };

            return View(cursos);
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
}
