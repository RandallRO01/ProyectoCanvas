namespace ProyectoCanvas.Models
{
    public class GrupoEstudiante
    {
        public int GrupoId { get; set; }
        public Grupo Grupo { get; set; }

        public int PersonaId { get; set; }
        public Persona Persona { get; set; }
    }
}
