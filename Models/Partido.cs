namespace ProdePrediccionesAPI.Models
{
    public class Partido
    {
        public int Id { get; set; }

        public int EquipoLocalId { get; set; }
        public Equipo? EquipoLocal { get; set; }

        public int EquipoVisitanteId { get; set; }
        public Equipo? EquipoVisitante { get; set; }

        public DateTime Fecha { get; set; }

        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }

        public string Estado { get; set; } = "Pendiente"; 
    }
}