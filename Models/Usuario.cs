using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoCanvas.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        [MaxLength(20)]
        public string Correo { get; set; }
        [MaxLength (20)]
        public string PasswordHash { get; set; } 
        public int Id_Persona { get; set; }
        public Persona Persona { get; set; }
    }
}
