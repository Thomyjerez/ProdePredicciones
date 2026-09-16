using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdePrediccionesAPI.Data;
using ProdePrediccionesAPI.Models;

namespace ProdePrediccionesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(usuario);
        }

[HttpGet("ranking")]
public async Task<ActionResult<IEnumerable<object>>> GetRanking()
{
    var ranking = await _context.Usuarios
        .Select(u => new
        {
            u.Id,
            u.Nombre,
            PuntosTotales = _context.Predicciones
                .Where(p => p.UsuarioId == u.Id)
                .Sum(p => p.PuntosObtenidos)
        })
        .OrderByDescending(r => r.PuntosTotales)
        .ToListAsync();

    return Ok(ranking);
}
    }
}