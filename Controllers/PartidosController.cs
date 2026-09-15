using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdePrediccionesAPI.Data;
using ProdePrediccionesAPI.Models;

namespace ProdePrediccionesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartidosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PartidosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Partido>>> GetPartidos()
        {
            return await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Partido>> PostPartido(Partido partido)
        {
            _context.Partidos.Add(partido);
            await _context.SaveChangesAsync();

            return Ok(partido);
        }
    }
}