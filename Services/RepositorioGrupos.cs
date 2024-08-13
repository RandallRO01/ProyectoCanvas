using Dapper;
using Microsoft.Data.SqlClient;
using ProyectoCanvas.Models;
using System.Text.RegularExpressions;

namespace ProyectoCanvas.Services
{
    public interface IRepositorioGrupos
    {
        Task CrearGrupo(Grupo grupo);
        Task AgregarPersonaAGrupo(int personaId, int grupoId);
        Task EliminarPersonaDeGrupo(int personaId, int grupoId);
        Task<IEnumerable<Grupo>> ObtenerGruposPorCurso(int cursoId);
        Task<IEnumerable<Persona>> ObtenerPersonasPorGrupo(int grupoId);
    }


    public class RepositorioGrupos : IRepositorioGrupos
    {
        private readonly string connectionString;

        public RepositorioGrupos(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CrearGrupo(Grupo grupo)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"INSERT INTO Grupos (Nombre, CursoId) VALUES (@Nombre, @CursoId)";
            await connection.ExecuteAsync(query, grupo);
        }

        public async Task AgregarPersonaAGrupo(int personaId, int grupoId)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"INSERT INTO GrupoEstudiantes (PersonaId, GrupoId) VALUES (@PersonaId, @GrupoId)";
            await connection.ExecuteAsync(query, new { PersonaId = personaId, GrupoId = grupoId });
        }

        public async Task EliminarPersonaDeGrupo(int personaId, int grupoId)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"DELETE FROM GrupoEstudiantes WHERE PersonaId = @PersonaId AND GrupoId = @GrupoId";
            await connection.ExecuteAsync(query, new { PersonaId = personaId, GrupoId = grupoId });
        }

        public async Task<IEnumerable<Grupo>> ObtenerGruposPorCurso(int cursoId)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"SELECT * FROM Grupos WHERE CursoId = @CursoId";
            return await connection.QueryAsync<Grupo>(query, new { CursoId = cursoId });
        }

        public async Task<IEnumerable<Persona>> ObtenerPersonasPorGrupo(int grupoId)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"
            SELECT p.* FROM Persona p
            INNER JOIN GrupoEstudiantes pg ON p.Id = pg.PersonaId
            WHERE pg.GrupoId = @GrupoId";
            return await connection.QueryAsync<Persona>(query, new { GrupoId = grupoId });
        }

    }

}
