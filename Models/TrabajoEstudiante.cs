namespace ProyectoCanvas.Models
{
    public class TrabajoEstudiante
    {
        public int Id { get; set; }
        public int Id_Asignacion { get; set; }
        public int Id_Estudiante { get; set; }
        public string NombreArchivo { get; set; }
        public byte[] Archivo { get; set; }
        public DateTime FechaSubida { get; set; } = DateTime.Now;
        public int? Calificacion { get; set; }

        // Relaciones
        public Asignacion Asignacion { get; set; }

        public string NombreEstudiante { get; set; }
    }
}
