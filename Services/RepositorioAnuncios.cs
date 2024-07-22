using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoCanvas.Models;

namespace ProyectoCanvas.Services
{
    public interface IRepositorioAnuncios
    {
        Task<IEnumerable<Anuncio>> ObtenerAnuncios();
        Task CrearAnuncio(Anuncio anuncio);
        Task<IEnumerable<Anuncio>> ObtenerAnunciosPorCurso(int idCurso);
    }

    public class RepositorioAnuncios : IRepositorioAnuncios
    {
        private readonly string connectionString;

        public RepositorioAnuncios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Anuncio>> ObtenerAnuncios()
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"
            SELECT a.*, p.Id, p.Nombre, p.Apellido_Paterno, p.Apellido_Materno
            FROM Anuncios a
            INNER JOIN Persona p ON a.PersonaId = p.Id";

            var anuncios = await connection.QueryAsync<Anuncio, Persona, Anuncio>(
                query,
                (anuncio, persona) =>
                {
                    anuncio.Persona = persona;
                    return anuncio;
                },
                splitOn: "Id");

            return anuncios;
        }

        
        public async Task CrearAnuncio(Anuncio anuncio)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"
        INSERT INTO Anuncios (Titulo, Descripcion, FechaPublicacion, Id_Persona, Id_Curso)
        VALUES (@Titulo, @Descripcion, @FechaPublicacion, @Id_Persona, @Id_Curso)";

            await connection.ExecuteAsync(query, anuncio);
        }


        public async Task<IEnumerable<Anuncio>> ObtenerAnunciosPorCurso(int cursoId)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"
            SELECT a.Id, a.Titulo, a.Descripcion, a.FechaPublicacion, a.Id_Persona, a.Id_Curso,
                   p.Id AS PersonaId, p.Nombre, p.Apellido_Paterno, p.Apellido_Materno
            FROM Anuncios a
            INNER JOIN Persona p ON a.Id_Persona = p.Id
            WHERE a.Id_Curso = @CursoId";

            var anuncios = await connection.QueryAsync<Anuncio, Persona, Anuncio>(
                query,
                (anuncio, persona) =>
                {
                    anuncio.Persona = persona;
                    return anuncio;
                },
                new { CursoId = cursoId },
                splitOn: "PersonaId"
            );

            return anuncios;
        }


    }
}
