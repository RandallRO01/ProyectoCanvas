using Dapper;
using Microsoft.Data.SqlClient;
using ProyectoCanvas.Models;
using ProyectoCanvas.ViewModels;

namespace ProyectoCanvas.Services
{
    public interface IRepositorioCursos
    {
        Task<IEnumerable<Cursos>> ObtenerCursos();
        Task<Cursos> ObtenerCursoPorId(int id);
        Task Crear(Cursos curso);
        Task Actualizar(Cursos curso);
        Task Eliminar(int id);
        Task<IEnumerable<EstudianteViewModel>> ObtenerEstudiantesConCorreoPorCurso(int idCurso);
    }

    public class RepositorioCursos : IRepositorioCursos
    {
        private readonly string _connectionString;

        public RepositorioCursos(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Cursos>> ObtenerCursos()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Cursos>("SELECT * FROM Cursos");
        }
        public async Task<Cursos> ObtenerCursoPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Cursos>(
                "SELECT * FROM Cursos WHERE Id = @Id", new { Id = id });
        }

        public async Task Crear(Cursos curso)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Cursos (NombreCurso, Descripcion, ImagenURL, Cuatrimestre, Year) 
                  VALUES (@NombreCurso, @Descripcion, @ImagenURL, @Cuatrimestre, @Year); 
                  SELECT SCOPE_IDENTITY();", curso);
            curso.Id = id;
        }
        public async Task Actualizar(Cursos curso)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                @"UPDATE Cursos SET 
                  NombreCurso = @NombreCurso, 
                  Descripcion = @Descripcion, 
                  ImagenUrl = @ImagenUrl, 
                  Cuatrimestre = @Cuatrimestre, 
                  Year = @Year 
                  WHERE Id = @Id", curso);
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("DELETE FROM Cursos WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<EstudianteViewModel>> ObtenerEstudiantesConCorreoPorCurso(int idCurso)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                SELECT p.Id, p.Nombre, p.Apellido_Paterno, p.Apellido_Materno, u.Correo
                FROM Persona p
                INNER JOIN Usuarios u ON p.Id = u.Id
                INNER JOIN Cursos ce ON p.Id = ce.Id
                WHERE ce.Id = @IdCurso";
                return await connection.QueryAsync<EstudianteViewModel>(query, new { IdCurso = idCurso });
            }
        }


    }
}
