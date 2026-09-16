using System.ComponentModel.DataAnnotations;

namespace ProdePrediccionesAPI.Models
{
    public class Prediccion
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int PartidoId { get; set; }
        public Partido? Partido { get; set; }

        [Range(0, 50, ErrorMessage = "Los goles no pueden ser negativos ni mayores a 50.")]
        public int GolesLocalPredichos { get; set; }

        [Range(0, 50, ErrorMessage = "Los goles no pueden ser negativos ni mayores a 50.")]
        public int GolesVisitantePredichos { get; set; }

        public int PuntosObtenidos { get; set; } = 0;
        
        public DateTime FechaPredicion { get; set; } = DateTime.UtcNow;
    }
}