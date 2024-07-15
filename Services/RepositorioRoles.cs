using Dapper;
using Microsoft.Data.SqlClient;

namespace ProyectoCanvas.Services
{
    public interface IRepositorioRoles
    {
        Task<bool> EsUsuarioEnRol(int usuarioId, string rol);
    }

    public class RepositorioRoles : IRepositorioRoles
    {
        private readonly string _connectionString;

        public RepositorioRoles(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> EsUsuarioEnRol(int usuarioId, string rol)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"SELECT COUNT(1) 
                          FROM Usuarios u
                          INNER JOIN Persona p ON u.Id_Persona = p.Id
                          INNER JOIN Roles r ON p.Id_rol = r.Id
                          WHERE u.Id = @UsuarioId AND r.Rol = @Rol";

            var count = await connection.ExecuteScalarAsync<int>(query, new { UsuarioId = usuarioId, Rol = rol });
            return count > 0;
        }
    }
}
