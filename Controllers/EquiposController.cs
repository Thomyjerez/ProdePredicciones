using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdePrediccionesAPI.Data;
using ProdePrediccionesAPI.Models;

namespace ProdePrediccionesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquiposController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquiposController(AppDbContext context)
        {
            _context = context;
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
    }
}