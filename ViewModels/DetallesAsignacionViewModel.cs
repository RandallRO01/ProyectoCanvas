using ProyectoCanvas.Models;

namespace ProyectoCanvas.ViewModels
{
    public class DetallesAsignacionViewModel
    {
        public Asignacion Asignacion { get; set; }
        public List<TrabajoEstudiante> TrabajosEstudiantes { get; set; }
        public bool HaSubidoTrabajo { get; set; }
        public bool FechaLimitePasada { get; set; }
        public DateTime? FechaLimite { get; set; }
    }
}
