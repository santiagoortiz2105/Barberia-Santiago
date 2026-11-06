using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Barberia.Api.Data;
using Barberia.Api.Domain.Entities;

namespace Barberia.Api.Controllers;

[ApiController]
[Route("api/sincronizacion")]
[Authorize]
public class SincronizacionController : ControllerBase
{
    private readonly BarberiaDbContext _context;

    public SincronizacionController(BarberiaDbContext context)
    {
        _context = context;
    }

    [HttpPost("turnos-pendientes")]
    public async Task<IActionResult> SincronizarTurnos([FromBody] List<TurnoSyncDto> turnosPendientes, CancellationToken cancellationToken)
    {
        if (turnosPendientes == null || turnosPendientes.Count == 0)
        {
            return Ok(new { mensaje = "No hay turnos pendientes para sincronizar." });
        }

        long usuarioIdToken = 0;
        var claimUsuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("id")?.Value;
        
        if (!string.IsNullOrEmpty(claimUsuarioId) && long.TryParse(claimUsuarioId, out long parsedId))
        {
            usuarioIdToken = parsedId;
        }

        if (usuarioIdToken <= 0 || !await _context.Set<Usuario>().AnyAsync(u => u.Id == usuarioIdToken, cancellationToken))
        {
            var primerUsuario = await _context.Set<Usuario>().OrderBy(u => u.Id).FirstOrDefaultAsync(cancellationToken);
            if (primerUsuario != null)
            {
                usuarioIdToken = primerUsuario.Id;
            }
            else
            {
                return BadRequest(new { exito = false, error = "No hay usuarios registrados en el sistema para asociar el turno." });
            }
        }

        var turnosAgregados = 0;

        try
        {
            foreach (var dto in turnosPendientes)
            {
                // Forzamos el uso del usuario validado para garantizar que exista en la BD
                long idUsuarioFinal = usuarioIdToken;
                
                var productoExiste = await _context.Set<Producto>().AnyAsync(p => p.Id == dto.ProductoId, cancellationToken);
                if (!productoExiste) continue; 

                var existe = await _context.Set<Turno>().AnyAsync(t => 
                    t.BarberoId == dto.BarberoId && 
                    t.FechaHora == dto.FechaHora && 
                    t.UsuarioId == idUsuarioFinal, cancellationToken);

                if (!existe)
                {
                    var nuevoTurno = new Turno
                    {
                        BarberoId = (dto.BarberoId > 0) ? dto.BarberoId : (long?)null,
                        ProductoId = dto.ProductoId,
                        UsuarioId = idUsuarioFinal,
                        FechaHora = dto.FechaHora,
                        Estado = "Reservado",
                        Activo = true
                    };

                    _context.Set<Turno>().Add(nuevoTurno);
                    turnosAgregados++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { 
                exito = true, 
                mensaje = $"Sincronización exitosa. Se guardaron {turnosAgregados} turnos nuevos en la base de datos." 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--- ERROR EN SINCRONIZACIÓN: {ex.Message} | Inner: {ex.InnerException?.Message} ---");
            return StatusCode(500, new { exito = false, error = ex.Message, inner = ex.InnerException?.Message });
        }
    }
} 

// DTO para recibir los datos desde la app móvil offline
public class TurnoSyncDto
{
    public long BarberoId { get; set; }
    public long ProductoId { get; set; }
    public long UsuarioId { get; set; }
    public DateTime FechaHora { get; set; } 
}