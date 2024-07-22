namespace ProyectoCanvas.ViewModels
{
    public class NotasDetalleViewModel
    {
        public DetallesAsignacionViewModel DetallesAsignacion { get; set; }
        public IEnumerable<CalificacionEstudianteViewModel> CalificacionesEstudiantes { get; set; }
    }
}
