using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdePrediccionesAPI.Data;
using ProdePrediccionesAPI.Models;

namespace ProdePrediccionesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrediccionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PrediccionesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prediccion>>> GetPredicciones()
        {
            return await _context.Predicciones
                .Include(p => p.Usuario)
                .Include(p => p.Partido)
                    .ThenInclude(pa => pa!.EquipoLocal)
                .Include(p => p.Partido)
                    .ThenInclude(pa => pa!.EquipoVisitante)
                .ToListAsync();
        }

[HttpGet("usuario/{usuarioId}")]
public async Task<ActionResult<IEnumerable<object>>> GetPrediccionesByUsuario(int usuarioId)
{
    var predicciones = await _context.Predicciones
        .Where(p => p.UsuarioId == usuarioId)
        .Include(p => p.Partido)
            .ThenInclude(pa => pa!.EquipoLocal)
        .Include(p => p.Partido)
            .ThenInclude(pa => pa!.EquipoVisitante)
        // Usamos Select para devolver un JSON "limpio" y fácil de leer para el frontend
        .Select(p => new {
            PrediccionId = p.Id,
            Partido = $"{p.Partido!.EquipoLocal!.Nombre} vs {p.Partido.EquipoVisitante!.Nombre}",
            TuPronostico = $"{p.GolesLocalPredichos} - {p.GolesVisitantePredichos}",
            ResultadoReal = p.Partido.Estado == "Finalizado" 
                ? $"{p.Partido.GolesLocal} - {p.Partido.GolesVisitante}" 
                : "Pendiente",
            PuntosGanados = p.PuntosObtenidos,
            FechaApuesta = p.FechaPredicion
        })
        .OrderByDescending(p => p.FechaApuesta)
        .ToListAsync();

    if (!predicciones.Any())
    {
        return NotFound("Este usuario todavía no hizo ninguna predicción.");
    }

    return Ok(predicciones);
}


[HttpPost]
public async Task<ActionResult<Prediccion>> PostPrediccion(Prediccion prediccion)
{
    var partido = await _context.Partidos.FindAsync(prediccion.PartidoId);
    if (partido == null)
    {
        return NotFound("El partido que intentás predecir no existe.");
    }

    if (partido.Fecha <= DateTime.UtcNow)
    {
        return BadRequest("¡El partido ya empezó o ya terminó! Ya no se aceptan pronósticos.");
    }

    var prediccionExistente = await _context.Predicciones
        .FirstOrDefaultAsync(p => p.UsuarioId == prediccion.UsuarioId && p.PartidoId == prediccion.PartidoId);
        
    if (prediccionExistente != null)
    {
        return BadRequest("Ya tenés un pronóstico guardado para este partido.");
    }

    _context.Predicciones.Add(prediccion);
    await _context.SaveChangesAsync();

    return Ok(prediccion);
}
    }
}