using Barberia.Api.Data;
using Barberia.Api.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QRCoder;

namespace Barberia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class TurnosController : ControllerBase
{
    private readonly BarberiaDbContext _context;

    public TurnosController(BarberiaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var rolClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "sinrol";
        
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        var query = _context.Turnos
            .Include(t => t.Producto)
            .AsQueryable();

        if (rolClaim == "sinrol" && long.TryParse(usuarioIdStr, out var usuarioId))
        {
            query = query.Where(t => t.UsuarioId == usuarioId);
        }

        var turnos = await query
            .OrderByDescending(t => t.FechaHora)
            .Select(t => new
            {
                t.Id,
                t.FechaHora,
                t.Estado,
                producto = t.Producto != null ? t.Producto.Nombre : "Servicio general",
                precio = t.Producto != null ? t.Producto.Precio : 0,
                t.UsuarioId
            })
            .ToListAsync(ct);

        return Ok(turnos);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] TurnoDto dto, CancellationToken ct)
    {
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!long.TryParse(usuarioIdStr, out var usuarioId))
        {
            return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
        }

        var turno = new Turno
        {
            UsuarioId = usuarioId,
            ProductoId = dto.ProductoId,
            FechaHora = dto.FechaHora,
            Estado = "Reservado",
            Activo = true
        };

        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Turno reservado con éxito", turnoId = turno.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancelar(long id, CancellationToken ct)
    {
        var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (turno == null)
        {
            return NotFound(new { mensaje = "El turno no fue encontrado." });
        }

        var rolClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "sinrol";
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (rolClaim == "sinrol" && long.TryParse(usuarioIdStr, out var usuarioId))
        {
            if (turno.UsuarioId != usuarioId)
            {
                return Forbid();
            }
        }

        turno.Estado = "Cancelado";
        turno.Activo = false;
        await _context.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Turno cancelado correctamente" });
    }

    [HttpGet("{id}/qr")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerQrTurno(long id, CancellationToken ct)
    {
        var turno = await _context.Turnos
            .Include(t => t.Producto)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (turno == null)
        {
            return NotFound(new { mensaje = "Turno no encontrado para generar QR." });
        }

        string nombreServicio = turno.Producto != null ? turno.Producto.Nombre : "Servicio general";
        decimal precioServicio = turno.Producto != null ? turno.Producto.Precio : 0;

        string textoQr = $"=== BARBERÍA SANTIAGO ===\n" +
                         $"Turno ID: {turno.Id}\n" +
                         $"Servicio: {nombreServicio}\n" +
                         $"Precio: ${precioServicio}\n" +
                         $"Fecha: {turno.FechaHora:dd/MM/yyyy HH:mm}\n" +
                         $"Estado: {turno.Estado}";

        using (var qrGenerator = new QRCodeGenerator())
        {
            var qrCodeData = qrGenerator.CreateQrCode(textoQr, QRCodeGenerator.ECCLevel.Q);
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeBytes = qrCode.GetGraphic(20);
                return File(qrCodeBytes, "image/png");
            }
        }
    }
}

public class TurnoDto
{
    public long ProductoId { get; set; }
    public DateTime FechaHora { get; set; }
}