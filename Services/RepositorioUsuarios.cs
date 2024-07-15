using Dapper;
using Microsoft.Data.SqlClient;
using ProyectoCanvas.Models;
using ProyectoCanvas.Services.Utilities;

namespace ProyectoCanvas.Services
{

    public interface IRepositorioUsuarios
    {
        Task<Usuario> ObtenerPorCorreo(string correo);
        Task Crear(Usuario usuario);
    }

    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string _connectionString;

        public RepositorioUsuarios(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Usuario> ObtenerPorCorreo(string correo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Usuario>(
                @"SELECT Id, Correo, PasswordHash, Id_Persona as IdPersona 
                  FROM Usuarios 
                  WHERE Correo = @Correo", new { Correo = correo });
        }

        public async Task Crear(Usuario usuario)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Usuarios (Correo, PasswordHash, Id_Persona) 
                  VALUES (@Correo, @PasswordHash, @IdPersona); 
                  SELECT SCOPE_IDENTITY();", usuario);
            usuario.Id = id;
        }
    }
}
