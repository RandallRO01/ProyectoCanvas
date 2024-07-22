using Microsoft.AspNetCore.Authorization;
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
        private readonly IRepositorioRoles _repositorioRoles;
        private readonly IRepositorioAsignaciones _repositorioAsignaciones;
        private readonly ILogger<HomeController> _logger;
        private readonly IRepositorioAsistencias _repositorioAsistencias;
        private readonly IRepositorioAnuncios _repositorioAnuncios;

        public CursosController(IRepositorioCursos repositorioCursos, IRepositorioRoles repositorioRoles, 
            IRepositorioAsignaciones repositorioAsignaciones,ILogger<HomeController> logger,
            IRepositorioAsistencias repositorioAsistencias, IRepositorioAnuncios repositorioAnuncios)
        {
            _repositorioCursos = repositorioCursos;
            _repositorioRoles = repositorioRoles;
            _repositorioAsignaciones = repositorioAsignaciones;
            _logger = logger;
            _repositorioAsistencias = repositorioAsistencias;
            _repositorioAnuncios = repositorioAnuncios;
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

            int cursoId = asignacion.Id_Curso;
            await SetCourseViewBag(cursoId);

            await _repositorioAsignaciones.EliminarAsignacion(id);
            return RedirectToAction("Asignaciones", new { id = cursoId });
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
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool esProfesor = await _repositorioRoles.EsUsuarioEnRol(usuarioId, "Profesor");
            ViewBag.EsProfesor = esProfesor;

            var asignacion = await _repositorioAsignaciones.ObtenerAsignacionPorId(id);
            var trabajosEstudiantes = await _repositorioAsignaciones.ObtenerTrabajosPorAsignacion(id);

            var viewModel = new DetallesAsignacionViewModel
            {
                Asignacion = asignacion,
                TrabajosEstudiantes = (List<TrabajoEstudiante>)trabajosEstudiantes,
                HaSubidoTrabajo = trabajosEstudiantes.Any(t => t.Id_Estudiante == usuarioId),
                FechaLimitePasada = asignacion.FechaLimite < DateTime.Now,
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

            // Obtener calificaciones
            var calificaciones = await _repositorioAsignaciones.ObtenerCalificacionesPorCurso(id);

            var calificacionesEstudiantes = calificaciones.Select(c => new CalificacionEstudianteViewModel
            {
                EstudianteId = c.EstudianteId,
                NombreCompleto = c.NombreCompleto,
                Correo = c.Correo,
                NotaTotal = c.NotaTotal,
                Asignaciones = c.Asignaciones.Select(a => new AsignacionCalificacionViewModel
                {
                    Id = a.Id,
                    NombreAsignacion = a.NombreAsignacion,
                    FechaEnvio = a.FechaEnvio,
                    Puntaje = a.Puntaje,
                    PuntajeMaximo = a.PuntajeMaximo
                }).ToList()
            }).ToList();

            // Crear y poblar el modelo de vista
            var viewModel = new NotasDetalleViewModel
            {
                CalificacionesEstudiantes = calificacionesEstudiantes
            };

            return View(viewModel);
        }


        public async Task<IActionResult> DetallesNotas(int idEstudiante, int idCurso)
        {
            var trabajos = await _repositorioAsignaciones.ObtenerTrabajosPorEstudianteYCurso(idEstudiante, idCurso);
            var viewModel = trabajos.Select(t => new TrabajoEstudianteViewModel
            {
                NombreAsignacion = t.Asignacion.Nombre,
                FechaSubida = t.FechaSubida,
                Calificacion = (int)t.Calificacion,
                PuntajeMaximo = t.Asignacion.TotalPuntos
            }).ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> Asistencia(int id)
        {
            await SetCourseViewBag(id);
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool esProfesor = await _repositorioRoles.EsUsuarioEnRol(usuarioId, "Profesor");

            ViewBag.EsProfesor = esProfesor;

            if (esProfesor)
            {
                var asistencias = await _repositorioAsistencias.ObtenerAsistenciasPorCurso(id);
                var estudiantes = await _repositorioCursos.ObtenerEstudiantesConCorreoPorCurso(id);

                var viewModel = estudiantes.Select(estudiante => new AsistenciaViewModel
                {
                    EstudianteId = estudiante.Id,
                    NombreCompleto = $"{estudiante.Nombre} {estudiante.ApellidoPaterno} {estudiante.ApellidoMaterno}",
                    Correo = estudiante.Correo,
                    Asistencias = asistencias.Where(a => a.PersonaId == estudiante.Id).ToList()
                }).ToList();

                return View(viewModel);
            }
            else
            {
                var asistencias = await _repositorioAsistencias.ObtenerAsistenciasPorEstudianteYCurso(id, usuarioId);
                var estudiante = await _repositorioAsistencias.ObtenerEstudiantePorId(usuarioId); // Método que obtenga los datos del estudiante

                var viewModel = new AsistenciaEstudianteViewModel
                {
                    EstudianteId = usuarioId,
                    NombreCompleto = $"{estudiante.Nombre} {estudiante.ApellidoPaterno} {estudiante.ApellidoMaterno}",
                    Asistencias = (List<Asistencia>)asistencias,
                    TotalAusencias = asistencias.Count(a => a.Estado == EstadoAsistencia.Ausente),
                    TotalPresentes = asistencias.Count(a => a.Estado == EstadoAsistencia.Presente),
                    TotalTardias = asistencias.Count(a => a.Estado == EstadoAsistencia.Tardia)
                };

                return View(viewModel);
            }
        }




        [HttpPost]
        public async Task<IActionResult> TomarAsistencia(int cursoId, List<AsistenciaInputModel> asistencias)
        {
            foreach (var asistenciaInput in asistencias)
            {
                var asistencia = new Asistencia
                {
                    PersonaId = asistenciaInput.EstudianteId,
                    CursoId = cursoId,
                    Fecha = DateTime.Today,
                    Estado = (EstadoAsistencia)asistenciaInput.Estado
                };

                await _repositorioAsistencias.CrearAsistencia(asistencia);
            }

            return RedirectToAction("Asistencia", new { id = cursoId });
        }     


        [HttpPost]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> GuardarAsistencia(int estudianteId, int cursoId, DateTime fecha, EstadoAsistencia estado)
        {
            var asistencia = new Asistencia
            {
                PersonaId = estudianteId,
                CursoId = cursoId,
                Fecha = fecha,
                Estado = estado
            };

            await _repositorioAsistencias.ActualizarAsistencia(asistencia);

            return Ok();
        }

        //Anuncios
        public async Task<IActionResult> Anuncios(int id)
        {
            await SetCourseViewBag(id);

            var anuncios = await _repositorioAnuncios.ObtenerAnunciosPorCurso(id);

            var viewModel = anuncios.Select(anuncio => new AnuncioViewModel
            {
                Id = anuncio.Id,
                Titulo = anuncio.Titulo,
                Descripcion = anuncio.Descripcion,
                FechaPublicacion = anuncio.FechaPublicacion,
                NombreProfesor = $"{anuncio.Persona.Nombre} {anuncio.Persona.ApellidoPaterno} {anuncio.Persona.ApellidoMaterno}"
            }).ToList();

            ViewBag.CourseId = id;
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CrearAnuncio(Anuncio anuncio)
        {
            if (ModelState.IsValid)
            {
                anuncio.FechaPublicacion = DateTime.Now;
                anuncio.Id_Persona = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _repositorioAnuncios.CrearAnuncio(anuncio);
                return RedirectToAction("Anuncios", new { id = anuncio.Id_Curso });
            }
            return RedirectToAction("Anuncios", new { id = anuncio.Id_Curso });
        }

        public async Task<IActionResult> Personas(int id)
        {
            await SetCourseViewBag(id);
            return View();
        }
    }
}
