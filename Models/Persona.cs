using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ProyectoCanvas.Models
{
    public class Persona
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public DateTime FechaNac { get; set; }
        public int Id_rol { get; set; }
        //public Role Rol { get; set; }
        public string Rol { get; set; }
        public ICollection<PersonaCurso> PersonaCursos { get; set; }
    }
}
