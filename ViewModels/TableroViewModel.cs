using ProyectoCanvas.Models;

namespace ProyectoCanvas.ViewModels
{
    public class TableroViewModel
    {
        public IEnumerable<Cursos> Cursos { get; set; }
        public IEnumerable<AsignacionPendienteViewModel> AsignacionesPendientes { get; set; }
    }
}
