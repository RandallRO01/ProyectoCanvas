using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoCanvas.Models
{
    public class PersonaCurso
    {
        public int Id_Persona { get; set; }
        public int Id_Curso { get; set; }
        public Persona Persona { get; set; }
        public Cursos Curso { get; set; }
    }
}
