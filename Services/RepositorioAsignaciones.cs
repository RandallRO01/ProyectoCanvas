using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProyectoCanvas.Models;
using ProyectoCanvas.ViewModels;

namespace ProyectoCanvas.Services
{
    public interface IRepositorioAsignaciones
    {
        Task<IEnumerable<Asignacion>> ObtenerAsignacionesPorCurso(int idCurso);
        Task<Asignacion> ObtenerAsignacionPorId(int id);
        Task CrearAsignacion(Asignacion asignacion);
        Task ActualizarAsignacion(Asignacion asignacion);
        Task EliminarAsignacion(int id);
        Task<IEnumerable<TrabajoEstudiante>> ObtenerTrabajosPorAsignacion(int idAsignacion);
        Task<TrabajoEstudiante> ObtenerTrabajoPorId(int id);
        Task CrearTrabajo(TrabajoEstudiante trabajo);
        Task ActualizarCalificacion(TrabajoEstudiante trabajo);
        Task EliminarTrabajo(int id);
        Task<IEnumerable<CalificacionEstudianteViewModel>> ObtenerCalificacionesPorCurso(int idCurso);
        Task<IEnumerable<TrabajoEstudiante>> ObtenerTrabajosPorEstudianteYCurso(int idEstudiante, int idCurso);
        Task<IEnumerable<CalificacionAsignacion>> ObtenerCalificacionesPorEstudiante(int estudianteId, int cursoId);
    }

    public class RepositorioAsignaciones : IRepositorioAsignaciones
    {
        private readonly string connectionString;

        public RepositorioAsignaciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Asignacion>> ObtenerAsignacionesPorCurso(int idCurso)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT * FROM Asignaciones WHERE Id_Curso = @Id_Curso";
                return await connection.QueryAsync<Asignacion>(query, new { Id_Curso = idCurso });
            }
        }

        public async Task<Asignacion> ObtenerAsignacionPorId(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT * FROM Asignaciones WHERE Id = @Id";
                return await connection.QueryFirstOrDefaultAsync<Asignacion>(query, new { Id = id });
            }
        }

        public async Task CrearAsignacion(Asignacion asignacion)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                INSERT INTO Asignaciones (Nombre, Descripcion, Url, Archivo, FechaLimite, Semana, Id_Curso, Id_Persona, TotalPuntos)
                VALUES (@Nombre, @Descripcion, @Url, @Archivo, @FechaLimite, @Semana, @Id_Curso, @Id_Persona, @TotalPuntos)";
                        await connection.ExecuteAsync(query, asignacion);
            }
        }

        public async Task ActualizarAsignacion(Asignacion asignacion)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    UPDATE Asignaciones
                    SET Nombre = @Nombre,
                        Descripcion = @Descripcion,
                        Url = @Url,
                        Archivo = @Archivo,
                        FechaLimite = @FechaLimite,
                        Semana = @Semana,
                        Id_Curso = @Id_Curso,
                        Id_Persona = @Id_Persona,
                        TotalPuntos = @TotalPuntos
                    WHERE Id = @Id";
                await connection.ExecuteAsync(query, asignacion);
            }
        }

        public async Task EliminarAsignacion(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var queryEliminarTrabajos = @"DELETE FROM TrabajosEstudiantes WHERE Id_Asignacion = @Id";
                var queryEliminarAsignacion = @"DELETE FROM Asignaciones WHERE Id = @Id";

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await connection.ExecuteAsync(queryEliminarTrabajos, new { Id = id }, transaction);
                        await connection.ExecuteAsync(queryEliminarAsignacion, new { Id = id }, transaction);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Métodos para manejar los trabajos de los estudiantes
        public async Task<IEnumerable<TrabajoEstudiante>> ObtenerTrabajosPorAsignacion(int idAsignacion)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                SELECT te.*, p.Id AS PersonaId, p.Nombre, p.Apellido_Paterno
                FROM TrabajosEstudiantes te
                INNER JOIN Persona p ON te.Id_Estudiante = p.Id
                WHERE te.Id_Asignacion = @Id_Asignacion";
                return await connection.QueryAsync<TrabajoEstudiante, Persona, TrabajoEstudiante>(
                    query,
                    (trabajo, persona) =>
                    {
                        trabajo.NombreEstudiante = persona.Nombre;
                        return trabajo;
                    },
                    new { Id_Asignacion = idAsignacion },
                    splitOn: "PersonaId"); // Asegúrate de que "PersonaId" esté alineado con el conjunto de resultados
            }
        }


        public async Task<TrabajoEstudiante> ObtenerTrabajoPorId(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT * FROM TrabajosEstudiantes WHERE Id = @Id";
                return await connection.QueryFirstOrDefaultAsync<TrabajoEstudiante>(query, new { Id = id });
            }
        }

        public async Task CrearTrabajo(TrabajoEstudiante trabajo)
        {
            using var connection = new SqlConnection(connectionString);
            string query = @"INSERT INTO TrabajosEstudiantes (Id_Asignacion, Id_Estudiante, NombreArchivo, Archivo, FechaSubida, Calificacion)
                     VALUES (@Id_Asignacion, @Id_Estudiante, @NombreArchivo, @Archivo, @FechaSubida, @Calificacion)";
            await connection.ExecuteAsync(query, new
            {
                trabajo.Id_Asignacion,
                trabajo.Id_Estudiante,
                trabajo.NombreArchivo,
                trabajo.Archivo,
                trabajo.FechaSubida,
                trabajo.Calificacion
            });
        }

        public async Task ActualizarCalificacion(TrabajoEstudiante trabajo)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"UPDATE TrabajosEstudiantes SET Calificacion = @Calificacion WHERE Id = @Id";
                await connection.ExecuteAsync(query, new { trabajo.Calificacion, trabajo.Id });
            }
        }

        public async Task EliminarTrabajo(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "DELETE FROM TrabajosEstudiantes WHERE Id = @Id";
                await connection.ExecuteAsync(query, new { Id = id });
            }
        }

        //Notas
        public async Task<IEnumerable<CalificacionEstudianteViewModel>> ObtenerCalificacionesPorCurso(int idCurso)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    SELECT 
                        p.Id AS EstudianteId,
                        p.Nombre + ' ' + p.Apellido_Paterno + ' ' + p.Apellido_Materno AS NombreCompleto,
                        u.Correo,
                        ISNULL(SUM(te.Calificacion), 0) AS NotaTotal,
                        a.Id AS AsignacionId,
                        a.Nombre,
                        te.FechaSubida AS FechaEnvio,
                        te.Calificacion AS Puntaje,
                        a.TotalPuntos AS PuntajeMaximo
                    FROM 
                        Persona p
                        INNER JOIN Usuarios u ON p.Id = u.Id_Persona
                        LEFT JOIN TrabajosEstudiantes te ON p.Id = te.Id_Estudiante
                        LEFT JOIN Asignaciones a ON te.Id_Asignacion = a.Id
                    WHERE 
                        a.Id_Curso = @CursoId
                    GROUP BY 
                        p.Id, p.Nombre, p.Apellido_Paterno, p.Apellido_Materno, u.Correo, a.Id, a.Nombre, te.FechaSubida, te.Calificacion, a.TotalPuntos
                    ORDER BY 
                        p.Nombre, p.Apellido_Paterno;";


                var calificacionesDict = new Dictionary<int, CalificacionEstudianteViewModel>();

                var result = await connection.QueryAsync<CalificacionEstudianteViewModel, AsignacionCalificacionViewModel, CalificacionEstudianteViewModel>(
                query,
                (estudiante, asignacion) =>
                {
                    if (!calificacionesDict.TryGetValue(estudiante.EstudianteId, out var calificacionEstudiante))
                    {
                        calificacionEstudiante = estudiante;
                        calificacionEstudiante.Asignaciones = new List<AsignacionCalificacionViewModel>();
                        calificacionesDict.Add(estudiante.EstudianteId, calificacionEstudiante);
                    }

                    calificacionEstudiante.Asignaciones.Add(asignacion);
                    return calificacionEstudiante;
                },
                splitOn: "AsignacionId",
                param: new { CursoId = idCurso });


                return calificacionesDict.Values.ToList();
            }
        }

        public async Task<IEnumerable<CalificacionAsignacion>> ObtenerCalificacionesPorEstudiante(int estudianteId, int cursoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            SELECT a.Nombre, te.Calificacion AS Puntaje, a.TotalPuntos AS PuntajeMaximo, te.FechaSubida AS FechaEnvio 
            FROM TrabajosEstudiantes te 
            INNER JOIN Asignaciones a ON te.Id_Asignacion = a.Id
            WHERE te.Id_Estudiante = @EstudianteId AND a.Id_Curso = @CursoId";

                return await connection.QueryAsync<CalificacionAsignacion>(query, new { EstudianteId = estudianteId, CursoId = cursoId });
            }
        }


        public async Task<IEnumerable<TrabajoEstudiante>> ObtenerTrabajosPorEstudianteYCurso(int idEstudiante, int idCurso)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            SELECT te.Id, te.FechaSubida, te.Calificacion, te.Id_Estudiante, te.Id_Asignacion,
                   a.Id as AsignacionId, a.Nombre as NombreAsignacion, a.TotalPuntos as PuntajeMaximo
            FROM TrabajosEstudiantes te
            INNER JOIN Asignaciones a ON te.Id_Asignacion = a.Id
            WHERE te.Id_Estudiante = @IdEstudiante AND a.Id_Curso = @IdCurso";

                var trabajos = await connection.QueryAsync<TrabajoEstudiante, Asignacion, TrabajoEstudiante>(
                    query,
                    (trabajo, asignacion) =>
                    {
                        trabajo.Asignacion = asignacion;  // Asegúrate de que la asignación está siendo correctamente asociada
                        return trabajo;
                    },
                    new { IdEstudiante = idEstudiante, IdCurso = idCurso },
                    splitOn: "AsignacionId"
                );

                return trabajos;
            }
        }



    }
}
