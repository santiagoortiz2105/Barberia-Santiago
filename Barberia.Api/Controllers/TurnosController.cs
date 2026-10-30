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
public class TurnosController : ControllerBase
{
    private readonly BarberiaDbContext _context;

    public TurnosController(BarberiaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var rolClaim = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower() ?? "";
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (!long.TryParse(usuarioIdStr, out var usuarioIdActual))
        {
            return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
        }

        var query = _context.Turnos
            .Include(t => t.Producto)
            .Include(t => t.Usuario)
            .Include(t => t.Barbero)
            .AsQueryable();

        bool esAdminGlobal = rolClaim == "admin" || rolClaim == "administrador";
        bool esBarbero = rolClaim == "barbero";

        // Si es barbero, filtramos estrictamente por los turnos asignados a su BarberoId
        if (esBarbero)
        {
            query = query.Where(t => t.BarberoId == usuarioIdActual);
        }
        // Si es un cliente común (no admin ni barbero), filtramos por los turnos que él reservó
        else if (!esAdminGlobal)
        {
            query = query.Where(t => t.UsuarioId == usuarioIdActual);
        }
        // Si es Admin, la consulta no se filtra y trae todos los turnos.

        var turnos = await query
            .OrderByDescending(t => t.FechaHora)
            .Select(t => new
            {
                t.Id,
                t.FechaHora,
                t.Estado,
                producto = t.Producto != null ? t.Producto.Nombre : "Servicio general",
                precio = t.Producto != null ? t.Producto.Precio : 0,
                t.UsuarioId,
                t.BarberoId,
                clienteNombre = t.Usuario != null ? t.Usuario.Nombre : "Usuario",
                barberoNombre = t.Barbero != null ? t.Barbero.Nombre : "Por asignar"
            })
            .ToListAsync(ct);

        return Ok(turnos);
    }

    [HttpGet("bandeja")]
    [Authorize]
    public async Task<IActionResult> ObtenerBandeja(CancellationToken ct)
    {
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!long.TryParse(usuarioIdStr, out var usuarioId))
        {
            return Unauthorized();
        }

        var notificaciones = await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.FechaCreacion)
            .ToListAsync(ct);

        return Ok(notificaciones);
    }

    [HttpPatch("{id}/leer")]
    [Authorize]
    public async Task<IActionResult> MarcarComoLeida(long id, CancellationToken ct)
    {
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!long.TryParse(usuarioIdStr, out var usuarioId))
        {
            return Unauthorized();
        }

        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == usuarioId, ct);

        if (notificacion == null)
        {
            return NotFound(new { mensaje = "Notificación no encontrada." });
        }

        notificacion.Leida = true;
        await _context.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Notificación marcada como leída" });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Crear([FromBody] TurnoDto dto, CancellationToken ct)
    {
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                            ?? User.FindFirst("sub")?.Value 
                            ?? User.FindFirst("id")?.Value 
                            ?? User.FindFirst("uid")?.Value;

        long usuarioId = 0;
        if (!long.TryParse(usuarioIdStr, out usuarioId))
        {
            var identifierFallback = User.FindFirst(ClaimTypes.Name)?.Value 
                                    ?? User.FindFirst(ClaimTypes.Email)?.Value 
                                    ?? usuarioIdStr;

            var usuarioDb = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == identifierFallback || u.Nombre == identifierFallback, ct);

            if (usuarioDb != null)
            {
                usuarioId = usuarioDb.Id;
            }
            else
            {
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión para crear el turno." });
            }
        }

        // Combinamos la fecha elegida con la hora del bloque seleccionado (ej: "15:00 - 15:40")
        DateTime fechaHoraFinal = dto.FechaHora;
        if (!string.IsNullOrWhiteSpace(dto.Horario))
        {
            try 
            {
                var horaInicioStr = dto.Horario.Split('-')[0].Trim(); // "15:00"
                var partesHora = horaInicioStr.Split(':');
                int hora = int.Parse(partesHora[0]);
                int minuto = int.Parse(partesHora[1]);

                fechaHoraFinal = new DateTime(dto.FechaHora.Year, dto.FechaHora.Month, dto.FechaHora.Day, hora, minuto, 0);
            }
            catch 
            {
                fechaHoraFinal = dto.FechaHora;
            }
        }

        var turno = new Turno
        {
            UsuarioId = (int)usuarioId,
            ProductoId = (int)dto.ProductoId,
            BarberoId = dto.BarberoId > 0 ? (int?)dto.BarberoId : null,
            FechaHora = fechaHoraFinal,
            Estado = "Reservado",
            Activo = true
        };

        _context.Turnos.Add(turno);

        // --- NOTIFICACIÓN PARA EL BARBERO / ADMINISTRADOR ---
        var barberosYAdmins = await _context.Usuarios
            .Where(u => u.Rol != null && (
                u.Rol.ToLower() == "admin" || 
                u.Rol.ToLower() == "administrador" ||
                (dto.BarberoId > 0 && u.Id == dto.BarberoId))) 
            .ToListAsync(ct);

        if (!barberosYAdmins.Any())
        {
            barberosYAdmins = await _context.Usuarios.Take(1).ToListAsync(ct);
        }

        foreach (var staff in barberosYAdmins)
        {
            _context.Notificaciones.Add(new Notificacion
            {
                UsuarioId = (int)staff.Id, 
                Titulo = "¡Nuevo Turno Reservado!",
                Mensaje = $"Un cliente ha reservado un turno para el {fechaHoraFinal:dd/MM/yyyy} a las {dto.Horario}.",
                Leida = false,
                FechaCreacion = DateTime.Now
            });
        }
        
        await _context.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Turno reservado con éxito", turnoId = turno.Id });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Cancelar(long id, CancellationToken ct)
    {
        var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (turno == null)
        {
            return NotFound(new { mensaje = "El turno no fue encontrado." });
        }

        var rolClaim = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower() ?? "";
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        bool esStaff = rolClaim == "admin" || rolClaim == "administrador" || rolClaim == "barbero";

        if (!esStaff && long.TryParse(usuarioIdStr, out var usuarioId))
        {
            if (turno.UsuarioId != usuarioId)
            {
                return Forbid();
            }
        }

        _context.Turnos.Remove(turno);
        await _context.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Turno eliminado correctamente" });
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

    [HttpDelete("notificacion/{id}")]
    [Authorize]
    public async Task<IActionResult> BorrarNotificacion(long id, CancellationToken ct)
    {
        var notificacion = await _context.Notificaciones.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (notificacion == null)
        {
            return NotFound(new { mensaje = "Notificación no encontrada" });
        }

        _context.Notificaciones.Remove(notificacion);
        await _context.SaveChangesAsync(ct);
        return Ok(new { mensaje = "Notificación eliminada correctamente" });
    }
}

public class TurnoDto
{
    public long ProductoId { get; set; }
    public long BarberoId { get; set; }
    public string Horario { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
}