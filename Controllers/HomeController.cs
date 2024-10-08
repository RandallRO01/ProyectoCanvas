using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Models;
using ProyectoCanvas.Services;
using ProyectoCanvas.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace ProyectoCanvas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepositorioRoles _repositorioRoles;
        private readonly IRepositorioCursos _repositorioCursos;
        private readonly IRepositorioAsignaciones _repositorioAsignaciones;
        private readonly IRepositorioAnuncios _repositorioAnuncios;

        public HomeController(ILogger<HomeController> logger, IRepositorioRoles repositorioRoles,
            IRepositorioCursos repositorioCursos, IRepositorioAsignaciones repositorioAsignaciones, IRepositorioAnuncios repositorioAnuncios)
        {
            _logger = logger;
            _repositorioRoles = repositorioRoles;
            _repositorioCursos = repositorioCursos;
            _repositorioAsignaciones = repositorioAsignaciones;
            _repositorioAnuncios = repositorioAnuncios;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool esProfesor = await _repositorioRoles.EsUsuarioEnRol(usuarioId, "Profesor");
            ViewBag.EsProfesor = esProfesor;

            var cursos = await _repositorioCursos.ObtenerCursos();
            var asignacionesPendientes = await _repositorioAsignaciones.ObtenerAsignacionesPendientesPorEstudiante(usuarioId);

            var viewModel = new TableroViewModel
            {
                Cursos = cursos,
                AsignacionesPendientes = asignacionesPendientes
            };

            // Pasar el nombre del usuario autenticado a la vista
            ViewBag.UserName = User.Identity.Name;

            return View(viewModel);
        }


        [HttpPost]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> CreateOrEditCourse(Cursos curso)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(curso.ImagenUrl))
                {
                    curso.ImagenUrl = "https://th.bing.com/th/id/OIP.ijWNEiPVAtBLrzw6F-yZxgAAAA?rs=1&pid=ImgDetMain";
                }

                if (curso.Id == 0)
                {
                    await _repositorioCursos.Crear(curso);
                }
                else
                {
                    await _repositorioCursos.Actualizar(curso);
                }
                return RedirectToAction("Index");
            }

            return View("Index");
        }

        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> EditCourse(int id)
        {
            var curso = await _repositorioCursos.ObtenerCursoPorId(id);
            if (curso == null)
            {
                return NotFound();
            }
            return Json(curso); // Devuelve el curso como JSON
        }

        [HttpPost]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> EditCourse(Cursos curso)
        {
            if (ModelState.IsValid)
            {
                await _repositorioCursos.Actualizar(curso);
                return RedirectToAction("Index");
            }
            return View(curso);
        }

        [HttpPost]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            await _repositorioCursos.Eliminar(id);
            return RedirectToAction("Index");
        }

        //Anuncios
        public async Task<IActionResult> Anuncios()
        {
            var anuncios = await _repositorioAnuncios.ObtenerTodosLosAnuncios();
            var viewModel = anuncios.Select(anuncio => new AnuncioViewModel
            {
                Id = anuncio.Id,
                Titulo = anuncio.Titulo,
                Descripcion = anuncio.Descripcion,
                FechaPublicacion = anuncio.FechaPublicacion,
                NombreProfesor = $"{anuncio.NombreProfesor}"
            }).ToList();

            return View(viewModel);
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
