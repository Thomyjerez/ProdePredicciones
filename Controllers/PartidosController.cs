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

        [HttpPut("{id}")]
public async Task<IActionResult> PutPartido(int id, Partido partidoActualizado)
{
    
    if (id != partidoActualizado.Id)
    {
        return BadRequest("El ID de la URL no coincide con el del cuerpo.");
    }

    var partidoDB = await _context.Partidos.FindAsync(id);
    if (partidoDB == null)
    {
        return NotFound("El partido no existe en la base de datos.");
    }

    if (partidoDB.Estado == "Finalizado")
    {
        return BadRequest("Este partido ya fue finalizado y los puntos ya se repartieron.");
    }

    partidoDB.GolesLocal = partidoActualizado.GolesLocal;
    partidoDB.GolesVisitante = partidoActualizado.GolesVisitante;
    partidoDB.Estado = "Finalizado";

    var predicciones = await _context.Predicciones
        .Include(p => p.Usuario)
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
        
        if (pred.Usuario != null)
        {
            pred.Usuario.PuntosTotales += puntos;
        }
    }

    await _context.SaveChangesAsync();

    return Ok(partidoDB);
}

[HttpDelete("{id}")]
public async Task<IActionResult> DeletePartido(int id)
{
    var partido = await _context.Partidos.FindAsync(id);
    if (partido == null)
    {
        return NotFound("El partido no existe.");
    }

    var prediccionesAsociadas = _context.Predicciones.Where(p => p.PartidoId == id);
    _context.Predicciones.RemoveRange(prediccionesAsociadas);

    _context.Partidos.Remove(partido);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Partido y predicciones asociadas eliminados correctamente." });
}

        [HttpPut("{id}/resultado")]
        public async Task<IActionResult> ActualizarResultado(int id, [FromBody] Partido partidoActualizado)
        {
            var partido = await _context.Partidos.FindAsync(id);
            if (partido == null)
            {
                return NotFound("Partido no encontrado.");
            }

            partido.GolesLocal = partidoActualizado.GolesLocal;
            partido.GolesVisitante = partidoActualizado.GolesVisitante;
            partido.Estado = "Finalizado";

            await _context.SaveChangesAsync();

            var predicciones = await _context.Predicciones
                .Where(p => p.PartidoId == id)
                .ToListAsync();

            foreach (var pred in predicciones)
            {
                int puntos = 0;

                if (pred.GolesLocalPredichos == partido.GolesLocal && pred.GolesVisitantePredichos == partido.GolesVisitante)
                {
                    puntos = 5; 
                }
                else
                {
                    bool prediccioLocalGana = pred.GolesLocalPredichos > pred.GolesVisitantePredichos;
                    bool partidoLocalGana = partido.GolesLocal > partido.GolesVisitante;

                    bool prediccionEmpate = pred.GolesLocalPredichos == pred.GolesVisitantePredichos;
                    bool partidoEmpate = partido.GolesLocal == partido.GolesVisitante;

                    bool prediccionVisitaGana = pred.GolesLocalPredichos < pred.GolesVisitantePredichos;
                    bool partidoVisitaGana = partido.GolesLocal < partido.GolesVisitante;

                    if ((prediccioLocalGana && partidoLocalGana) ||
                        (prediccionEmpate && partidoEmpate) ||
                        (prediccionVisitaGana && partidoVisitaGana))
                    {
                        puntos = 3; 
                    }
                }

                pred.PuntosObtenidos = puntos;
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Resultado actualizado y puntos calculados correctamente", partido });
        }
    }
}