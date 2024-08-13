namespace ProyectoCanvas.Models
{
    public class Grupo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int CursoId { get; set; }

        // Relación con Curso
        public Cursos Curso { get; set; }

        public Persona Persona { get; set; }
        public List<Persona> Personas { get; set; } = new List<Persona>();

        // Relación con Estudiantes en el grupo
        public ICollection<GrupoEstudiante> GrupoEstudiantes { get; set; }
    }
}
