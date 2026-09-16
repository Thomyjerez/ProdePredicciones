namespace ProdePrediccionesAPI.Models
{
    public class Prediccion
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int PartidoId { get; set; }
        public Partido? Partido { get; set; }

        public int GolesLocalPredichos { get; set; }
        public int GolesVisitantePredichos { get; set; }

        public int PuntosObtenidos { get; set; } = 0;
        
        public DateTime FechaPredicion { get; set; } = DateTime.UtcNow;
    }
}