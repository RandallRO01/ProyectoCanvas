namespace ProyectoCanvas.Models
{
    public class Cursos
    {
        public int Id { get; set; }
        public string NombreCurso { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; } = "https://th.bing.com/th/id/OIP.ijWNEiPVAtBLrzw6F-yZxgAAAA?rs=1&pid=ImgDetMain";
        public string Cuatrimestre { get; set; }
        public int Year { get; set; }
        public ICollection<PersonaCurso> PersonaCursos { get; set; }
    }
}
