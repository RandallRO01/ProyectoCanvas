using Dapper;
using Microsoft.Data.SqlClient;
using ProyectoCanvas.Models;

namespace ProyectoCanvas.Services
{
    public interface IRepositorioAsistencias
    {
        Task<IEnumerable<Asistencia>> ObtenerAsistenciasPorCurso(int cursoId);
        Task<IEnumerable<Asistencia>> ObtenerAsistenciasPorEstudiante(int estudianteId);
        Task CrearAsistencia(Asistencia asistencia);
        Task ActualizarAsistencia(Asistencia asistencia);
        Task<IEnumerable<Asistencia>> ObtenerAsistenciasPorEstudianteYCurso(int estudianteId, int cursoId);
        Task<Persona> ObtenerEstudiantePorId(int id);
        Task<IEnumerable<Asistencia>> ObtenerAsistenciasPorEstudianteYFecha(int cursoId, int estudianteId, DateTime fecha);
        Task<IEnumerable<dynamic>> ObtenerAsistenciasPorCursoYFecha(int cursoId, DateTime fecha);
    }

    public class RepositorioAsistencia : IRepositorioAsistencias
    {
        private readonly string connectionString;

        public RepositorioAsistencia(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Asistencia>> ObtenerAsistenciasPorCurso(int cursoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    SELECT * FROM Asistencias
                    WHERE CursoId = @CursoId";

                return await connection.QueryAsync<Asistencia>(query, new { CursoId = cursoId });
            }
        }

        public async Task<IEnumerable<Asistencia>> ObtenerAsistenciasPorEstudiante(int estudianteId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    SELECT * FROM Asistencias
                    WHERE PersonaId = @PersonaId";

                return await connection.QueryAsync<Asistencia>(query, new { PersonaId = estudianteId });
            }
        }

        public async Task CrearAsistencia(Asistencia asistencia)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    INSERT INTO Asistencias (PersonaId, CursoId, Fecha, Estado)
                    VALUES (@PersonaId, @CursoId, @Fecha, @Estado)";

                await connection.ExecuteAsync(query, asistencia);
            }
        }

        public async Task ActualizarAsistencia(Asistencia asistencia)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            UPDATE Asistencias
            SET Estado = @Estado, Fecha = @Fecha
            WHERE PersonaId = @PersonaId AND CursoId = @CursoId AND Id = @Id";

                await connection.ExecuteAsync(query, asistencia);
            }
        }


        public async Task<IEnumerable<Asistencia>> ObtenerAsistenciasPorEstudianteYCurso(int estudianteId, int cursoId)
        {
            using var connection = new SqlConnection(connectionString);
            var query = "SELECT * FROM Asistencias WHERE PersonaId = @EstudianteId AND CursoId = @CursoId";
            return await connection.QueryAsync<Asistencia>(query, new { EstudianteId = estudianteId, CursoId = cursoId });
        }

        public async Task<Persona> ObtenerEstudiantePorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var query = "SELECT * FROM Persona WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Persona>(query, new { Id = id });
        }

        public async Task<IEnumerable<dynamic>> ObtenerAsistenciasPorCursoYFecha(int cursoId, DateTime fecha)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    SELECT 
                        a.PersonaId,
                        p.Nombre + ' ' + p.Apellido_Paterno AS NombreCompleto,
                        u.Correo,
                        a.Estado
                    FROM 
                        Asistencias a
                    INNER JOIN 
                        Persona p ON a.PersonaId = p.Id
                    INNER JOIN 
                        Usuarios u ON p.Id = u.Id_Persona
                    WHERE 
                        a.CursoId = @CursoId 
                        AND a.Fecha = @Fecha";

                return await connection.QueryAsync<dynamic>(query, new { CursoId = cursoId, Fecha = fecha });
            }
        }

        public async Task<IEnumerable<Asistencia>> ObtenerAsistenciasPorEstudianteYFecha(int cursoId, int estudianteId, DateTime fecha)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                SELECT * FROM Asistencias 
                WHERE CursoId = @CursoId AND PersonaId = @EstudianteId AND Fecha = @Fecha";

                return await connection.QueryAsync<Asistencia>(query, new { CursoId = cursoId, EstudianteId = estudianteId, Fecha = fecha });
            }
        }
    }
}

