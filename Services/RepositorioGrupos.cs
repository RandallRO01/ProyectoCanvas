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
        Task<Grupo> ObtenerGrupoPorId(int grupoId);
        Task ActualizarGrupo(Grupo grupo);
        Task EliminarEstudiantesDeGrupo(int grupoId);
        Task EliminarGrupo(int grupoId);
        Task<IEnumerable<Persona>> ObtenerPersonasEnGruposPorCurso(int cursoId);
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
            var query = @"INSERT INTO Grupos (Nombre, CursoId) OUTPUT INSERTED.Id VALUES (@Nombre, @CursoId)";
            grupo.Id = await connection.QuerySingleAsync<int>(query, grupo);
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
            var query = @"
                SELECT g.*, p.*
                FROM Grupos g
                LEFT JOIN GrupoEstudiantes ge ON g.Id = ge.GrupoId
                LEFT JOIN Persona p ON ge.PersonaId = p.Id
                WHERE g.CursoId = @CursoId";

            var grupoDictionary = new Dictionary<int, Grupo>();

            var result = await connection.QueryAsync<Grupo, Persona, Grupo>(query, (grupo, persona) =>
            {
                if (!grupoDictionary.TryGetValue(grupo.Id, out var grupoEntry))
                {
                    grupoEntry = grupo;
                    grupoEntry.Personas = new List<Persona>();
                    grupoDictionary.Add(grupo.Id, grupoEntry);
                }

                if (persona != null)
                {
                    grupoEntry.Personas.Add(persona);
                }

                return grupoEntry;
            }, new { CursoId = cursoId });

            return grupoDictionary.Values;
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


        public async Task<Grupo> ObtenerGrupoPorId(int grupoId)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"SELECT * FROM Grupos WHERE Id = @GrupoId";
            return await connection.QuerySingleOrDefaultAsync<Grupo>(query, new { GrupoId = grupoId });
        }

        public async Task ActualizarGrupo(Grupo grupo)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"UPDATE Grupos SET Nombre = @Nombre WHERE Id = @Id";
            await connection.ExecuteAsync(query, grupo);
        }

        public async Task EliminarEstudiantesDeGrupo(int grupoId)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"DELETE FROM GrupoEstudiantes WHERE GrupoId = @GrupoId";
            await connection.ExecuteAsync(query, new { GrupoId = grupoId });
        }

        public async Task EliminarGrupo(int grupoId)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"DELETE FROM Grupos WHERE Id = @GrupoId";
            await connection.ExecuteAsync(query, new { GrupoId = grupoId });
        }

        public async Task<IEnumerable<Persona>> ObtenerPersonasEnGruposPorCurso(int cursoId)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"
                SELECT p.* FROM Persona p
                INNER JOIN GrupoEstudiantes ge ON p.Id = ge.PersonaId
                INNER JOIN Grupos g ON ge.GrupoId = g.Id
                WHERE g.CursoId = @CursoId";

            return await connection.QueryAsync<Persona>(query, new { CursoId = cursoId });
        }



    }

}
