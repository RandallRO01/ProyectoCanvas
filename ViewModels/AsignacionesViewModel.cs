using ProyectoCanvas.Models;

namespace ProyectoCanvas.ViewModels
{
    public class AsignacionesViewModel
    {
        public Cursos Curso { get; set; }
        public Dictionary<int, List<Asignacion>> AsignacionesPorSemana { get; set; }
        public bool EsProfesor { get; set; }
    }
}
