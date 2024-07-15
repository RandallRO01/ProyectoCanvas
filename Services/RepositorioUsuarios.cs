using Dapper;
using Microsoft.Data.SqlClient;
using ProyectoCanvas.Models;

namespace ProyectoCanvas.Services
{

    public interface IRepositorioUsuarios
    {
        Task<Usuario> ObtenerPorCorreo(string correo);
        Task Crear(Usuario usuario);
    }

    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string connectionString;

        public RepositorioUsuarios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Usuario> ObtenerPorCorreo(string correo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Usuario>(
                @"SELECT Id, Correo, PasswordHash, Id_Persona as IdPersona 
              FROM Usuarios 
              WHERE Correo = @Correo", new { Correo = correo });
        }

        public async Task Crear(Usuario usuario)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Usuarios (Correo, PasswordHash, Id_Persona) 
              VALUES (@Correo, @PasswordHash, @IdPersona); 
              SELECT SCOPE_IDENTITY();", usuario);
            usuario.Id = id;
        }
    }
}
