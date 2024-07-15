namespace ProyectoCanvas.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Rol { get; set; }
        public ICollection<Persona> Personas { get; set; }
    }
}
