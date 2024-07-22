using ProyectoCanvas.Models;

namespace ProyectoCanvas.ViewModels
{
    public class AsistenciaViewModel
    {
        public int EstudianteId { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public List<Asistencia> Asistencias { get; set; }
    }
}
