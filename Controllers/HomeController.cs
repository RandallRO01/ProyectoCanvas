using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Models;
using System.Diagnostics;

namespace ProyectoCanvas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            
        }

        [Authorize]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Correo") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Role = HttpContext.Session.GetString("Role");

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
