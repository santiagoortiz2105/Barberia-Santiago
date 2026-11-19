using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Barberia.Api.Data;
using Barberia.Api.Domain.Entities;
using System.Security.Claims;

namespace Barberia.Api.Controllers;

[ApiController]
[Route("api/dispositivos")]
[Authorize]
public class DispositivosController : ControllerBase
{
    private readonly BarberiaDbContext _context;

    public DispositivosController(BarberiaDbContext context)
    {
        _context = context;
    }

    [HttpPut]
    public async Task<IActionResult> RegistrarToken([FromBody] DispositivoDto dto)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(usuarioIdClaim) || !long.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(dto.Token))
        {
            return BadRequest("El token es obligatorio.");
        }

        var existente = await _context.DispositivosPush
            .FirstOrDefaultAsync(d => d.UsuarioId == usuarioId || d.Token == dto.Token);

        if (existente != null)
        {
            existente.Token = dto.Token;
            existente.ActualizadoEn = DateTime.UtcNow;
            _context.DispositivosPush.Update(existente);
        }
        else
        {
            var nuevo = new DispositivoPush
            {
                UsuarioId = usuarioId,
                Token = dto.Token,
                ActualizadoEn = DateTime.UtcNow,
                Activo = true
            };
            _context.DispositivosPush.Add(nuevo);
        }

        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Dispositivo registrado correctamente para push." });
    }
}

public class DispositivoDto
{
    public string Token { get; set; } = string.Empty;
}