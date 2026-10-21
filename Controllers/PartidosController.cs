using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdePrediccionesAPI.Data;
using ProdePrediccionesAPI.Models;

namespace ProdePrediccionesAPI.Controllers
{
    public class ResultadoPartidoDto
    {
        public int GolesLocal { get; set; }
        public int GolesVisitante { get; set; }
    }

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

        [HttpPut("{id}/finalizar")]
        public async Task<IActionResult> FinalizarPartido(int id, [FromBody] ResultadoPartidoDto resultado)
        {
            var partidoDB = await _context.Partidos.FindAsync(id);
            if (partidoDB == null) return NotFound("El partido no existe en la base de datos.");
            if (partidoDB.Estado == "Finalizado") return BadRequest("Este partido ya fue finalizado y los puntos ya se repartieron.");
            partidoDB.GolesLocal = resultado.GolesLocal;
            partidoDB.GolesVisitante = resultado.GolesVisitante;
            partidoDB.Estado = "Finalizado";

            var predicciones = await _context.Predicciones
                .Where(p => p.PartidoId == id)
                .ToListAsync();
            foreach (var pred in predicciones)
            {
                int puntos = 0;
                
                bool aciertoExacto = pred.GolesLocalPredichos == partidoDB.GolesLocal && 
                                     pred.GolesVisitantePredichos == partidoDB.GolesVisitante;

                bool ganoLocalReal = partidoDB.GolesLocal > partidoDB.GolesVisitante;
                bool ganoVisitaReal = partidoDB.GolesVisitante > partidoDB.GolesLocal;
                bool empateReal = partidoDB.GolesLocal == partidoDB.GolesVisitante;

                bool predijoLocal = pred.GolesLocalPredichos > pred.GolesVisitantePredichos;
                bool predijoVisita = pred.GolesVisitantePredichos > pred.GolesLocalPredichos;
                bool predijoEmpate = pred.GolesLocalPredichos == pred.GolesVisitantePredichos;

                bool aciertoTendencia = (ganoLocalReal && predijoLocal) || 
                                        (ganoVisitaReal && predijoVisita) || 
                                        (empateReal && predijoEmpate);

                if (aciertoExacto)
                {
                    puntos = 3; 
                }
                else if (aciertoTendencia)
                {
                    puntos = 1; 
                }

                pred.PuntosObtenidos = puntos;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Partido finalizado y puntos repartidos." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePartido(int id)
        {
            var partido = await _context.Partidos.FindAsync(id);
            if (partido == null) return NotFound("El partido no existe.");

            var prediccionesAsociadas = _context.Predicciones.Where(p => p.PartidoId == id);
            _context.Predicciones.RemoveRange(prediccionesAsociadas);

            _context.Partidos.Remove(partido);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Partido y predicciones asociadas eliminados correctamente." });
        }
    }
}