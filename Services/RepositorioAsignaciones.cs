using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProyectoCanvas.Models;

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
                var query = "DELETE FROM Asignaciones WHERE Id = @Id";
                await connection.ExecuteAsync(query, new { Id = id });
            }
        }

        // Métodos para manejar los trabajos de los estudiantes
        public async Task<IEnumerable<TrabajoEstudiante>> ObtenerTrabajosPorAsignacion(int idAsignacion)
        {
            using var connection = new SqlConnection(connectionString);
            string query = @"
                SELECT t.*, p.Nombre AS NombreEstudiante
                FROM TrabajosEstudiantes t
                INNER JOIN Persona p ON t.Id_Estudiante = p.Id
                WHERE t.Id_Asignacion = @Id_Asignacion";
            return await connection.QueryAsync<TrabajoEstudiante>(query, new { Id_Asignacion = idAsignacion });
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
    }
}
