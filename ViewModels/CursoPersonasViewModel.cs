using ProyectoCanvas.Models;

namespace ProyectoCanvas.ViewModels
{
    public class CursoPersonasViewModel
    {
        public int CursoId { get; set; }
        public IEnumerable<Persona> Personas { get; set; }
        public IEnumerable<Grupo> Grupos { get; set; }
        public Grupo GrupoSeleccionado { get; set; } // Asegúrate de que esta propiedad exista
        public IEnumerable<Persona> EstudiantesDisponibles { get; set; }
        public List<Persona> EstudiantesEnGrupos { get; set; }

    }
}
