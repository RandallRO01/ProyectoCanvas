using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Services;

namespace ProyectoCanvas.Controllers
{
    public class CursosController : Controller
    {
        private readonly IRepositorioCursos _repositorioCursos;
        private readonly ILogger<HomeController> _logger;

        public CursosController(IRepositorioCursos repositorioCursos, ILogger<HomeController> logger)
        {
            _repositorioCursos = repositorioCursos;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int id)
        {
            var curso = await _repositorioCursos.ObtenerCursoPorId(id);
            if (curso == null)
            {
                return NotFound();
            }
            ViewBag.CourseId = id;
            ViewBag.CourseName = curso.NombreCurso;
            ViewBag.CourseDescription = curso.Descripcion;
            return View(curso);
        }

        public async Task<IActionResult> Hogar(int id)
        {
            var curso = await _repositorioCursos.ObtenerCursoPorId(id);
            if (curso == null)
            {
                return NotFound();
            }
            ViewBag.CourseId = id;
            ViewBag.CourseName = curso.NombreCurso;
            ViewBag.CourseDescription = curso.Descripcion;
            ViewBag.CourseCuatri = curso.Cuatrimestre;
            return View(curso);
        }

        public IActionResult Asignaciones(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }
        public IActionResult Notas(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }
        public IActionResult Asistencia(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }
        public IActionResult Anuncios(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }
        public IActionResult Personas(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }
    }
}
