namespace ProdePrediccionesAPI.Models
{
    public class Equipo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string EscudoUrl { get; set; } = string.Empty;
    }
}