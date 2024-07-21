using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Models;
using ProyectoCanvas.Services;
using ProyectoCanvas.ViewModels;
using System.Security.Claims;

namespace ProyectoCanvas.Controllers
{
    public class CursosController : Controller
    {
        private readonly IRepositorioCursos _repositorioCursos;
        private readonly IRepositorioAsignaciones _repositorioAsignaciones;
        private readonly ILogger<HomeController> _logger;

        public CursosController(IRepositorioCursos repositorioCursos, IRepositorioAsignaciones repositorioAsignaciones,ILogger<HomeController> logger)
        {
            _repositorioCursos = repositorioCursos;
            _repositorioAsignaciones = repositorioAsignaciones;
            _logger = logger;
        }

        private async Task SetCourseViewBag(int id)
        {
            var curso = await _repositorioCursos.ObtenerCursoPorId(id);
            if (curso != null)
            {
                ViewBag.CourseId = id;
                ViewBag.CourseName = curso.NombreCurso;
                ViewBag.CourseDescription = curso.Descripcion;
                ViewBag.CourseCuatri = curso.Cuatrimestre;
            }
        }

        // Gestión del contenido de Hogar
        public async Task<IActionResult> Hogar(int id)
        {
            await SetCourseViewBag(id);
            var curso = await _repositorioCursos.ObtenerCursoPorId(id);
            if (curso == null)
            {
                return NotFound();
            }
            return View(curso);
        }

        public async Task<IActionResult> Asignaciones(int id)
        {
            await SetCourseViewBag(id);
            var curso = await _repositorioCursos.ObtenerCursoPorId(id);
            if (curso == null)
            {
                return NotFound();
            }

            var asignaciones = await _repositorioAsignaciones.ObtenerAsignacionesPorCurso(id);
            var asignacionesPorSemana = asignaciones
                .GroupBy(a => a.Semana)
                .ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new AsignacionesViewModel
            {
                Curso = curso,
                AsignacionesPorSemana = asignacionesPorSemana,
                EsProfesor = User.IsInRole("Profesor")
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CrearEditarAsignacion(Asignacion asignacion, IFormFile archivo)
        {
            if (ModelState.IsValid)
            {
                if (archivo != null && archivo.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await archivo.CopyToAsync(memoryStream);
                        asignacion.Archivo = memoryStream.ToArray();
                    }
                }

                if (asignacion.Id == 0)
                {
                    await _repositorioAsignaciones.CrearAsignacion(asignacion);
                }
                else
                {
                    await _repositorioAsignaciones.ActualizarAsignacion(asignacion);
                }

                return RedirectToAction("Asignaciones", new { id = asignacion.Id_Curso });
            }

            return View(asignacion);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarAsignacion(int id)
        {
            var asignacion = await _repositorioAsignaciones.ObtenerAsignacionPorId(id);
            if (asignacion == null)
            {
                return NotFound();
            }

            await _repositorioAsignaciones.EliminarAsignacion(id);

            return RedirectToAction("Asignaciones", new { id = asignacion.Id_Curso });
        }

        public async Task<IActionResult> ObtenerAsignacion(int id)
        {
            var asignacion = await _repositorioAsignaciones.ObtenerAsignacionPorId(id);
            if (asignacion == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = asignacion.Id,
                nombre = asignacion.Nombre,
                descripcion = asignacion.Descripcion,
                url = asignacion.Url,
                semana = asignacion.Semana,
                tieneArchivo = asignacion.Archivo != null,
                totalPuntos = asignacion.TotalPuntos,
                fechaLimite = asignacion.FechaLimite.HasValue ? asignacion.FechaLimite.Value.ToString("yyyy-MM-ddTHH:mm") : null
            });
        }

        public async Task<IActionResult> DetallesAsignacion(int id)
        {
            var asignacion = await _repositorioAsignaciones.ObtenerAsignacionPorId(id);
            if (asignacion == null)
            {
                return NotFound();
            }

            var trabajos = await _repositorioAsignaciones.ObtenerTrabajosPorAsignacion(id);

            bool haSubidoTrabajo = false;
            if (User.IsInRole("Estudiante"))
            {
                var estudianteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                haSubidoTrabajo = trabajos.Any(t => t.Id_Estudiante == estudianteId);
            }

            bool fechaLimitePasada = asignacion.FechaLimite.HasValue && DateTime.Now > asignacion.FechaLimite.Value;

            var viewModel = new DetallesAsignacionViewModel
            {
                Asignacion = asignacion,
                TrabajosEstudiantes = trabajos.ToList(),
                HaSubidoTrabajo = haSubidoTrabajo,
                FechaLimitePasada = fechaLimitePasada,
                FechaLimite = asignacion.FechaLimite
            };

            await SetCourseViewBag(asignacion.Id_Curso);
            return View(viewModel);
        }

        public async Task<IActionResult> DescargarAsignacion(int id)
        {
            var asignacion = await _repositorioAsignaciones.ObtenerAsignacionPorId(id);
            if (asignacion == null || asignacion.Archivo == null)
            {
                return NotFound();
            }

            string fileName = asignacion.Url ?? "archivo.pdf";
            string fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension))
            {
                return BadRequest("La URL del archivo no contiene una extensión válida.");
            }

            string mimeType = GetMimeType(fileExtension);

            return File(asignacion.Archivo, mimeType, fileName);
        }


        // Método auxiliar para obtener el tipo MIME basado en la extensión del archivo
        private string GetMimeType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream", // Tipo por defecto si no se encuentra una coincidencia
            };
        }

        // Métodos para gestionar los trabajos de los estudiantes
        public async Task<IActionResult> SubirTrabajoEstudiante(TrabajoEstudiante trabajo, IFormFile archivo)
        {
            if (ModelState.IsValid)
            {
                if (archivo != null && archivo.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await archivo.CopyToAsync(memoryStream);
                        trabajo.Archivo = memoryStream.ToArray();
                        trabajo.NombreArchivo = archivo.FileName;
                        trabajo.FechaSubida = DateTime.Now; // Asegurarse de asignar la fecha de subida
                    }
                }
                else
                {
                    ModelState.AddModelError("Archivo", "El archivo es obligatorio.");
                    return View("DetallesAsignacion", trabajo);
                }

                await _repositorioAsignaciones.CrearTrabajo(trabajo);

                return RedirectToAction("DetallesAsignacion", new { id = trabajo.Id_Asignacion });
            }

            return View(trabajo);
        }


        public async Task<IActionResult> DescargarTrabajoEstudiante(int id)
        {
            var trabajo = await _repositorioAsignaciones.ObtenerTrabajoPorId(id);
            if (trabajo == null || trabajo.Archivo == null)
            {
                return NotFound();
            }

            string fileName = trabajo.NombreArchivo;
            string fileExtension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                return BadRequest("La URL del archivo no contiene una extensión válida.");
            }

            string mimeType = GetMimeType(fileExtension);

            return File(trabajo.Archivo, mimeType, fileName);
        }

        public async Task<IActionResult> CalificarTrabajo(int id, int calificacion)
        {
            var trabajo = await _repositorioAsignaciones.ObtenerTrabajoPorId(id);
            if (trabajo == null)
            {
                return NotFound();
            }

            trabajo.Calificacion = calificacion;
            await _repositorioAsignaciones.ActualizarCalificacion(trabajo);

            return RedirectToAction("DetallesAsignacion", new { id = trabajo.Id_Asignacion });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarTrabajoEstudiante(int id)
        {
            var trabajo = await _repositorioAsignaciones.ObtenerTrabajoPorId(id);
            if (trabajo == null)
            {
                return NotFound();
            }

            await _repositorioAsignaciones.EliminarTrabajo(id);

            return RedirectToAction("DetallesAsignacion", new { id = trabajo.Id_Asignacion });
        }

        // Gestión del contenido de Notas
        public async Task<IActionResult> Notas(int id)
        {
            await SetCourseViewBag(id);
            return View();
        }

        public async Task<IActionResult> Asistencia(int id)
        {
            await SetCourseViewBag(id);
            return View();
        }

        public async Task<IActionResult> Anuncios(int id)
        {
            await SetCourseViewBag(id);
            return View();
        }

        public async Task<IActionResult> Personas(int id)
        {
            await SetCourseViewBag(id);
            return View();
        }
    }
}
