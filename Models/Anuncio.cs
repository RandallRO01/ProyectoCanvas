using System.ComponentModel.DataAnnotations;

namespace ProyectoCanvas.Models
{
    public class Anuncio
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public int Id_Persona { get; set; }
        public int Id_Curso { get; set; }
        public Persona Persona { get; set; }
    }
}
