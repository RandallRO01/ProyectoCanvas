using Microsoft.EntityFrameworkCore;
using ProyectoCanvas.Models;

namespace ProyectoCanvas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        { 
        }

        public DbSet<Cursos> Cursos { get; set; }

    }
}
