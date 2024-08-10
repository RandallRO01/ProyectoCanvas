using ProyectoCanvas.Models;

namespace ProyectoCanvas.ViewModels
{
    public class AsignacionCalificacionViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaEnvio { get; set; }
        public int Puntaje { get; set; }
        public int PuntajeMaximo { get; set; }
        public int CursoId { get; set; }
    }
}
