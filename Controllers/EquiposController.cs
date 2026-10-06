using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdePrediccionesAPI.Data;
using ProdePrediccionesAPI.Models;
using ProdePrediccionesAPI.Services;

namespace ProdePrediccionesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquiposController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FootballApiService _footballService; 
        public EquiposController(AppDbContext context, FootballApiService footballService)
        {
            _context = context;
            _footballService = footballService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipo>>> GetEquipos()
        {
            return await _context.Equipos.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Equipo>> PostEquipo(Equipo equipo)
        {
            _context.Equipos.Add(equipo);
            await _context.SaveChangesAsync();

            return Ok(equipo);
        }

        [HttpPost("importar/{ligaId}/{temporada}")]
        public async Task<IActionResult> ImportarEquipos(int ligaId, int temporada)
        {
            try
            {
                var equiposExternos = await _footballService.GetEquiposPorLigaAsync(ligaId, temporada);
                int agregados = 0;

                foreach (var ext in equiposExternos)
                {
                    bool existe = await _context.Equipos.AnyAsync(e => e.Nombre == ext.Nombre);
                    
                    if (!existe)
                    {
                        var nuevoEquipo = new Equipo
                        {
                            Nombre = ext.Nombre,
                            Pais = ext.Pais,
                            EscudoUrl = ext.EscudoUrl
                        };
                        
                        _context.Equipos.Add(nuevoEquipo);
                        agregados++;
                    }
                }

                if (agregados > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = $"Proceso finalizado. Se importaron {agregados} equipos nuevos." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al comunicarse con la API externa: {ex.Message}" });
            }
        }
    }
}