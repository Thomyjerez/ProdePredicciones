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

        [HttpGet("mis-predicciones")]
        public async Task<ActionResult<IEnumerable<object>>> GetMisPredicciones([FromQuery] string? nombreUsuario = null)
        {
            int usuarioId = 0;
            var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("id")?.Value
                       ?? User.FindFirst("usuarioId")?.Value
                       ?? User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(claimId) && int.TryParse(claimId, out int parsedId))
            {
                usuarioId = parsedId;
            }

            if (usuarioId == 0 && Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var tokenStr = authHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                
                if (handler.CanReadToken(tokenStr))
                {
                    var jwt = handler.ReadJwtToken(tokenStr);
                    var idEnToken = jwt.Claims.FirstOrDefault(c => 
                        (c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase) || 
                         c.Type == "id" || c.Type == "sub" || c.Type == "usuarioId") 
                        && int.TryParse(c.Value, out _))?.Value;

                    if (!string.IsNullOrEmpty(idEnToken))
                    {
                        usuarioId = int.Parse(idEnToken);
                    }
                    else
                    {
                        var nombreEnToken = jwt.Claims.FirstOrDefault(c => 
                            c.Type.EndsWith("name", StringComparison.OrdinalIgnoreCase) || 
                            c.Type == "unique_name" || c.Type == "sub")?.Value;

                        if (!string.IsNullOrEmpty(nombreEnToken))
                        {
                            var userDb = await _context.Usuarios
                                .FirstOrDefaultAsync(u => u.Nombre.ToLower() == nombreEnToken.ToLower());
                            if (userDb != null) usuarioId = userDb.Id;
                        }
                    }
                }
            }

            if (usuarioId == 0 && !string.IsNullOrEmpty(nombreUsuario))
            {
                var userDb = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Nombre.ToLower() == nombreUsuario.ToLower());
                if (userDb != null) usuarioId = userDb.Id;
            }

            if (usuarioId == 0)
            {
                return Ok(new List<object>());
            }

            var misPredicciones = await _context.Predicciones
                .Where(p => p.UsuarioId == usuarioId)
                .Select(p => new {
                    p.Id,
                    p.PartidoId,
                    p.GolesLocalPredichos,
                    p.GolesVisitantePredichos,
                    p.PuntosObtenidos
                })
                .ToListAsync();

            return Ok(misPredicciones);
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
    var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UsuarioId")?.Value;
    
    if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioIdToken))
    {
        return Unauthorized("Sesión inválida. Por favor, volvé a iniciar sesión.");
    }

    prediccion.UsuarioId = usuarioIdToken;

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