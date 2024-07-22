namespace ProyectoCanvas.ViewModels
{
    public class CalificacionEstudianteViewModel
    {
        public int EstudianteId { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public int NotaTotal { get; set; }
        public List<AsignacionCalificacionViewModel> Asignaciones { get; set; }
        public int CursoId { get; set; }
    }
}
