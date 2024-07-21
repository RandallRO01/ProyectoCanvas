namespace ProyectoCanvas.Models
{
    public class Asignacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Url { get; set; }
        public byte[] Archivo { get; set; }
        public int Semana { get; set; }
        public int Id_Curso { get; set; }
        public int Id_Persona { get; set; }
        public DateTime? FechaLimite { get; set; }
        public int TotalPuntos { get; set; }

        public List<TrabajoEstudiante> TrabajosEstudiantes { get; set; }
    }
}
