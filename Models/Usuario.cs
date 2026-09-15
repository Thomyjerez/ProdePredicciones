namespace ProdePrediccionesAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PuntosTotales { get; set; } = 0;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}