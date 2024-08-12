using Dapper;
using Microsoft.Data.SqlClient;
using ProyectoCanvas.Models;

namespace ProyectoCanvas.Services
{
    public interface IRepositorioPersonas
    {
        Task AgregarPersonaACurso(int personaId, int cursoId);
        Task EliminarPersonaDeCurso(int personaId, int cursoId);
        Task<IEnumerable<Persona>> ObtenerEstudiantesDisponibles(int cursoId);
        Task<IEnumerable<Persona>> ObtenerPersonasPorCurso(int cursoId);
    }

    public class RepositorioPersona : IRepositorioPersonas
    {
        private readonly string _connectionString;

        public RepositorioPersona(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Persona>> ObtenerPersonasPorCurso(int cursoId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
                SELECT p.Id, p.Nombre, p.Apellido_Paterno, p.Apellido_Materno, p.Fecha_Nac, r.Rol
                FROM Persona p
                INNER JOIN PersonaCursos pc ON p.Id = pc.Id_Persona
                INNER JOIN Roles r ON p.Id_rol = r.Id
                WHERE pc.Id_Curso = @CursoId";
            return await connection.QueryAsync<Persona>(query, new { CursoId = cursoId });
        }

        public async Task<IEnumerable<Persona>> ObtenerEstudiantesDisponibles(int cursoId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                SELECT * FROM Persona 
                WHERE Id NOT IN (SELECT Id_Persona FROM PersonaCursos WHERE Id_Curso = @CursoId)
                AND Id_rol = (SELECT Id FROM Roles WHERE Rol = 'Estudiante')";
                return await connection.QueryAsync<Persona>(query, new { CursoId = cursoId });
            }
        }

        public async Task AgregarPersonaACurso(int personaId, int cursoId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Verificar si la persona y el curso existen antes de insertar
                var curso = await connection.QueryFirstOrDefaultAsync<Cursos>("SELECT * FROM Cursos WHERE Id = @Id", new { Id = cursoId });
                var persona = await connection.QueryFirstOrDefaultAsync<Persona>("SELECT * FROM Persona WHERE Id = @Id", new { Id = personaId });

                if (curso != null && persona != null)
                {
                    string query = "INSERT INTO PersonaCursos (Id_Persona, Id_Curso) VALUES (@PersonaId, @CursoId)";
                    await connection.ExecuteAsync(query, new { PersonaId = personaId, CursoId = cursoId });
                }
                else
                {
                    throw new Exception("Curso o persona no existe.");
                }
            }
        }


        public async Task EliminarPersonaDeCurso(int personaId, int cursoId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"DELETE FROM PersonaCursos
                  WHERE Id_Persona = @PersonaId AND Id_Curso = @CursoId";
            await connection.ExecuteAsync(query, new { PersonaId = personaId, CursoId = cursoId });
        }

    }
}
