using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Barberia.Api.Data;

namespace Barberia.Api.Controllers
{
    [Route("api/barberos")]
    [ApiController]
    public class BarberosController : ControllerBase
    {
        private readonly BarberiaDbContext _context;

        public BarberosController(BarberiaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBarberos()
        {
            var barberos = await _context.Usuarios
                .Where(u => u.Rol.ToLower() == "barbero")
                .ToListAsync();

            return Ok(barberos);
        }

        [HttpPost("promover-barbero")]
        [Authorize(Roles = "Admin, admin")]
        public async Task<IActionResult> PromoverBarbero([FromBody] DtoEmail model)
        {
            if (string.IsNullOrWhiteSpace(model?.Email))
            {
                return BadRequest(new { mensaje = "El correo electrónico es requerido." });
            }

            var emailLimpio = model.Email.Trim().ToLower();
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLimpio);

            if (usuario == null)
            {
                return NotFound(new { mensaje = "No se encontró ningún usuario con ese correo electrónico." });
            }

            usuario.Rol = "Barbero";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario promovido a barbero exitosamente." });
        }
[HttpDelete("{id}")]
        [Authorize(Roles = "Admin, admin")]
        public async Task<IActionResult> QuitarBarbero(long id) 
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    return NotFound(new { mensaje = "Usuario no encontrado en la base de datos." });
                }

              
                usuario.Rol = "Cliente";
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Rol de barbero removido correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message, detalle = ex.InnerException?.Message ?? ex.ToString() });
            }
        }
    }

    public class DtoEmail
    {
        public string Email { get; set; } = string.Empty;
    }
}