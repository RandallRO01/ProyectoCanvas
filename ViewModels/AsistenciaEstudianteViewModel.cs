using ProyectoCanvas.Models;

namespace ProyectoCanvas.ViewModels
{
    public class AsistenciaEstudianteViewModel
    {
        public int EstudianteId { get; set; }
        public string NombreCompleto { get; set; }
        public List<Asistencia> Asistencias { get; set; }
        public int TotalAusencias { get; set; }
        public int TotalPresentes { get; set; }
        public int TotalTardias { get; set; }
    }
}
