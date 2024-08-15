using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoCanvas.Models;
using ProyectoCanvas.Services;
using ProyectoCanvas.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

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
        private readonly IRepositorioPersonas _repositorioPersonas;
        private readonly IRepositorioGrupos _repositorioGrupos;

        public CursosController(IRepositorioCursos repositorioCursos, IRepositorioRoles repositorioRoles,
            IRepositorioAsignaciones repositorioAsignaciones, ILogger<HomeController> logger,
            IRepositorioAsistencias repositorioAsistencias, IRepositorioAnuncios repositorioAnuncios, 
            IRepositorioPersonas repositorioPersonas, IRepositorioGrupos repositorioGrupos)
        {
            _repositorioCursos = repositorioCursos;
            _repositorioRoles = repositorioRoles;
            _repositorioAsignaciones = repositorioAsignaciones;
            _logger = logger;
            _repositorioAsistencias = repositorioAsistencias;
            _repositorioAnuncios = repositorioAnuncios;
            _repositorioPersonas = repositorioPersonas;
            _repositorioGrupos = repositorioGrupos;
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
                if (asignacion.FechaLimite < DateTime.Now)
                {
                    ModelState.AddModelError("FechaLimite", "La fecha límite no puede ser en el pasado.");
                    // Podrías retornar una vista aquí con los datos del curso y las asignaciones cargadas nuevamente
                    return View(asignacion);
                }

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

                return Json(new { success = true });
            }

            return Json(new { success = false });
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
            await SetCourseViewBag(id);

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
                fechaLimite = asignacion.FechaLimite.HasValue ? asignacion.FechaLimite.Value.ToString("yyyy-MM-ddTHH:mm") : null,
                nombreEstudiante = asignacion.TrabajosEstudiantes
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
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool esProfesor = await _repositorioRoles.EsUsuarioEnRol(usuarioId, "Profesor");

            if (esProfesor)
            {
                await SetCourseViewBag(id);
                // Obtener las calificaciones de todos los estudiantes en el curso
                var calificaciones = await _repositorioAsignaciones.ObtenerCalificacionesPorCurso(id);

                // Calcular la nota total como se hacía antes para los profesores
                var viewModel = new NotasDetalleViewModel
                {
                    CalificacionesEstudiantes = calificaciones.Select(c =>
                    {
                        double totalPuntosObtenidos = c.Asignaciones.Sum(a => a.Puntaje);
                        double totalPuntosPosibles = c.Asignaciones.Sum(a => a.PuntajeMaximo);
                        double promedio = totalPuntosPosibles > 0 ? (totalPuntosObtenidos / totalPuntosPosibles) * 100 : 0;

                        return new CalificacionEstudianteViewModel
                        {
                            EstudianteId = c.EstudianteId,
                            NombreCompleto = c.NombreCompleto,
                            Correo = c.Correo,
                            NotaTotal = (int)promedio,
                            Asignaciones = c.Asignaciones
                        };
                    }).ToList()
                };

                return View("Notas", viewModel); // Vista para profesores
            }
            else
            {
                await SetCourseViewBag(id);
                // Cálculo para el estudiante
                var calificaciones = await _repositorioAsignaciones.ObtenerCalificacionesPorEstudiante(usuarioId, id);
                double totalPuntosObtenidos = calificaciones.Sum(a => a.Puntaje);
                double totalPuntosPosibles = calificaciones.Sum(a => a.PuntajeMaximo);
                double promedio = totalPuntosPosibles > 0 ? (totalPuntosObtenidos / totalPuntosPosibles) * 100 : 0;

                var viewModel = new CalificacionEstudianteViewModel
                {
                    EstudianteId = usuarioId,
                    NombreCompleto = User.Identity.Name,
                    NotaTotal = (int)promedio,
                    Asignaciones = calificaciones.Select(a => new AsignacionCalificacionViewModel
                    {
                        Nombre = a.Nombre,
                        Puntaje = a.Puntaje,
                        PuntajeMaximo = a.PuntajeMaximo,
                        FechaEnvio = a.FechaEnvio
                    }).ToList()
                };

                await SetCourseViewBag(id);
                return View("NotasEstudiante", viewModel); // Vista para estudiantes
            }
        }



        public async Task<IActionResult> CalcularPromedio(int estudianteId, int cursoId)
        {
            // Obtener los trabajos del estudiante para el curso
            var trabajos = await _repositorioAsignaciones.ObtenerTrabajosPorEstudianteYCurso(estudianteId, cursoId);

            // Filtrar asignaciones que valen más de 0 y preparar los datos para el cálculo
            var trabajosValidos = trabajos.Where(t => t.Asignacion.TotalPuntos > 0).ToList();

            // Si no hay trabajos válidos, el promedio es 0
            if (!trabajosValidos.Any())
            {
                return Json(new { success = true, promedio = 0 });
            }

            // Calcular el total de puntos obtenidos y el total de puntos posibles
            double totalPuntosObtenidos = 0;
            double totalPuntosPosibles = 0;

            foreach (var trabajo in trabajosValidos)
            {
                totalPuntosObtenidos += trabajo.Calificacion ?? 0; // Si no hay calificación, cuenta como 0
                totalPuntosPosibles += trabajo.Asignacion.TotalPuntos;
            }

            // Calcular el promedio
            double promedio = (totalPuntosObtenidos / totalPuntosPosibles) * 100;

            // Devolver el resultado como JSON para mostrar en la vista
            return Json(new { success = true, promedio = promedio });
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

        public async Task<IActionResult> ObtenerAsistenciaPorFecha(int cursoId, DateTime fecha)
        {
            // Obtiene las asistencias de un curso y una fecha específica
            var asistencias = await _repositorioAsistencias.ObtenerAsistenciasPorCursoYFecha(cursoId, fecha);

            // Si existen asistencias, procesarlas para enviarlas a la vista
            if (asistencias != null && asistencias.Any())
            {
                // No necesitas hacer ningún mapeo adicional aquí porque ya tienes los datos en el formato adecuado
                return Json(new { success = true, data = asistencias });
            }
            else
            {
                return Json(new { success = false, message = "No se encontró asistencia para la fecha seleccionada." });
            }
        }



        [HttpPost]
        public async Task<IActionResult> GuardarAsistencia([FromBody] List<AsistenciaInputModel> asistencias)
        {
            if (asistencias == null || !asistencias.Any())
            {
                return BadRequest("No se proporcionaron datos de asistencia.");
            }

            foreach (var asistenciaInput in asistencias)
            {
                // Verificar si ya existe una asistencia para la combinación CursoId, EstudianteId, y Fecha
                var asistenciaExistente = await _repositorioAsistencias.ObtenerAsistenciasPorEstudianteYFecha(asistenciaInput.CursoId, asistenciaInput.EstudianteId, asistenciaInput.Fecha);

                if (asistenciaExistente != null && asistenciaExistente.Any())
                {
                    // Actualiza el estado de asistencia si ya existe
                    var asistencia = asistenciaExistente.First();
                    asistencia.Estado = (EstadoAsistencia)asistenciaInput.Estado;
                    await _repositorioAsistencias.ActualizarAsistencia(asistencia);
                }
                else
                {
                    // Crea una nueva asistencia si no existe
                    var nuevaAsistencia = new Asistencia
                    {
                        PersonaId = asistenciaInput.EstudianteId,
                        CursoId = asistenciaInput.CursoId,
                        Fecha = asistenciaInput.Fecha,
                        Estado = (EstadoAsistencia)asistenciaInput.Estado
                    };
                    await _repositorioAsistencias.CrearAsistencia(nuevaAsistencia);
                }
            }

            return Ok();
        }


        //Anuncios
        public async Task<IActionResult> Anuncios(int id)
        {
            await SetCourseViewBag(id);

            // Obtener los anuncios y ordenarlos por FechaPublicacion en orden descendente
            var anuncios = await _repositorioAnuncios.ObtenerAnunciosPorCurso(id);

            var viewModel = anuncios
                .OrderByDescending(anuncio => anuncio.FechaPublicacion) // Ordenar por FechaPublicacion en orden descendente
                .Select(anuncio => new AnuncioViewModel
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

        // Personas
        public async Task<IActionResult> Personas(int id, int? grupoId = null)
        {
            // Obtener todas las personas asociadas al curso
            var personas = await _repositorioPersonas.ObtenerPersonasPorCurso(id);

            // Obtener todos los grupos del curso
            var grupos = await _repositorioGrupos.ObtenerGruposPorCurso(id);

            // Obtener todos los estudiantes que ya están en grupos dentro del curso
            var estudiantesEnGrupos = await _repositorioGrupos.ObtenerPersonasEnGruposPorCurso(id);

            // Obtener los estudiantes disponibles (todos los estudiantes)
            var estudiantesDisponibles = await _repositorioPersonas.ObtenerEstudiantesDisponibles(id);

            Grupo grupoSeleccionado = null;
            if (grupoId.HasValue)
            {
                grupoSeleccionado = await _repositorioGrupos.ObtenerGrupoPorId(grupoId.Value);
            }

            // Crear el ViewModel y asignar las propiedades
            var viewModel = new CursoPersonasViewModel
            {
                Personas = personas,
                Grupos = grupos,
                GrupoSeleccionado = grupoSeleccionado,
                EstudiantesDisponibles = estudiantesDisponibles,
                EstudiantesEnGrupos = estudiantesEnGrupos.ToList()
            };

            ViewBag.CourseId = id;
            return View(viewModel);
        }


        public async Task<IActionResult> ObtenerEstudiantesDisponibles(int cursoId)
        {
            var estudiantes = await _repositorioPersonas.ObtenerEstudiantesDisponibles(cursoId);
            return PartialView("_EstudiantesDisponibles", estudiantes);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarEstudiante(int cursoId, int personaId)
        {
            // Verifica que el curso y la persona existan antes de intentar insertarlos
            var curso = await _repositorioCursos.ObtenerCursoPorId(cursoId);
            var persona = await _repositorioPersonas.ObtenerPersonasPorCurso(personaId);

            if (curso != null && persona != null)
            {
                await _repositorioPersonas.AgregarPersonaACurso(personaId, cursoId);
                return RedirectToAction("Personas", new { id = cursoId });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EliminarPersona(int cursoId, int personaId)
        {
            try
            {
                await _repositorioPersonas.EliminarPersonaDeCurso(personaId, cursoId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerGrupos(int cursoId)
        {
            var grupos = await _repositorioGrupos.ObtenerGruposPorCurso(cursoId);
            return Json(grupos);
        }

        [HttpPost("CrearGrupo")]
        public async Task<IActionResult> CrearGrupo(int cursoId, string nombreGrupo, List<int> estudiantesSeleccionados, int grupoId = 0)
        {
            if (string.IsNullOrEmpty(nombreGrupo))
            {
                return Json(new { success = false, message = "El nombre del grupo es requerido." });
            }

            try
            {
                if (grupoId > 0)
                {
                    // Actualizar grupo existente
                    var grupoExistente = await _repositorioGrupos.ObtenerGrupoPorId(grupoId);
                    if (grupoExistente == null)
                    {
                        return Json(new { success = false, message = "El grupo no existe." });
                    }

                    grupoExistente.Nombre = nombreGrupo;
                    await _repositorioGrupos.ActualizarGrupo(grupoExistente);

                    // Eliminar estudiantes actuales del grupo (limpiar tabla GrupoEstudiantes)
                    await _repositorioGrupos.EliminarEstudiantesDeGrupo(grupoId);

                    // Asignar estudiantes seleccionados al grupo
                    foreach (var estudianteId in estudiantesSeleccionados)
                    {
                        await _repositorioGrupos.AgregarPersonaAGrupo(estudianteId, grupoId);
                    }
                }
                else
                {
                    // Crear nuevo grupo
                    var grupo = new Grupo
                    {
                        Nombre = nombreGrupo,
                        CursoId = cursoId
                    };

                    await _repositorioGrupos.CrearGrupo(grupo);

                    // Asignar estudiantes al nuevo grupo
                    foreach (var estudianteId in estudiantesSeleccionados)
                    {
                        await _repositorioGrupos.AgregarPersonaAGrupo(estudianteId, grupo.Id);
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Ocurrió un error al guardar el grupo." });
            }
        }


        // Método para eliminar un grupo completo
        [HttpPost]
        public async Task<IActionResult> EliminarGrupo(int grupoId)
        {
            try
            {
                // Primero, elimina todas las relaciones de estudiantes con el grupo
                await _repositorioGrupos.EliminarEstudiantesDeGrupo(grupoId);

                // Luego, elimina el grupo
                await _repositorioGrupos.EliminarGrupo(grupoId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Ocurrió un error al eliminar el grupo." });
            }
        }


        // Método para eliminar una persona de un grupo
        [HttpPost]
        public async Task<IActionResult> EliminarPersonaDeGrupo(int grupoId, int personaId)
        {
            await _repositorioGrupos.EliminarPersonaDeGrupo(personaId, grupoId);
            return Json(new { success = true });
        }

        // Método para agregar una persona a un grupo
        [HttpPost]
        public async Task<IActionResult> AgregarPersonaAGrupo(int grupoId, int personaId)
        {
            await _repositorioGrupos.AgregarPersonaAGrupo(personaId, grupoId);
            return Json(new { success = true });
        }

        // Método para obtener las personas de un grupo específico (retorna JSON)
        public async Task<IActionResult> ObtenerPersonasPorGrupo(int grupoId)
        {
            var personas = await _repositorioGrupos.ObtenerPersonasPorGrupo(grupoId);
            return Json(personas);
        }
    }
}
