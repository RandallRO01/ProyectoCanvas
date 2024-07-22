using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoCanvas.Models
{
    public class Asistencia
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int CursoId { get; set; }
        public DateTime Fecha { get; set; }
        public EstadoAsistencia Estado { get; set; }
    }

    public enum EstadoAsistencia
    {
        Ausente = 0,
        Presente = 1,
        Tardia = 2
    }
}
