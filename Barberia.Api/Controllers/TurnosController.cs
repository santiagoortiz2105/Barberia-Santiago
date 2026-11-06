using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QRCoder;
using Barberia.Api.Data;
using Barberia.Api.Domain.Entities;

namespace Barberia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TurnosController : ControllerBase
{
    private readonly BarberiaDbContext _context;
    private readonly IConfiguration _configuration;

    public TurnosController(BarberiaDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var rolClaim = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower() ?? "";
        var usuarioId = ObtenerUsuarioIdActual();

        if (!usuarioId.HasValue)
        {
            return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión." });
        }

        var query = _context.Turnos
            .Include(t => t.Producto)
            .Include(t => t.Usuario)
            .Include(t => t.Barbero)
            .AsQueryable();

        bool esAdminGlobal = rolClaim.Contains("admin");
        bool esBarbero = rolClaim.Contains("barbero");

        if (esBarbero)
        {
            query = query.Where(t => t.BarberoId == usuarioId.Value);
        }
        else if (!esAdminGlobal)
        {
            query = query.Where(t => t.UsuarioId == usuarioId.Value);
        }

        var turnosDb = await query
            .OrderByDescending(t => t.FechaHora)
            .Select(t => new
            {
                t.Id,
                t.FechaHora,
                t.Estado,
                t.TokenPublico,
                producto = t.Producto != null ? t.Producto.Nombre : "Servicio general",
                precio = t.Producto != null ? t.Producto.Precio : 0,
                t.UsuarioId,
                t.BarberoId,
                clienteNombre = t.Usuario != null ? t.Usuario.Nombre : "Usuario",
                barberoNombre = t.Barbero != null ? t.Barbero.Nombre : "Por asignar"
            })
            .ToListAsync(ct);

        var turnos = turnosDb.Select(t => new
        {
            t.Id,
            t.FechaHora,
            t.Estado,
            t.TokenPublico,
            t.producto,
            t.precio,
            t.UsuarioId,
            t.BarberoId,
            t.clienteNombre,
            t.barberoNombre,
            qrUrl = ObtenerUrlDeepLink(t.TokenPublico)
        }).ToList();

        return Ok(turnos);
    }

    [HttpGet("bandeja")]
    [Authorize]
    public async Task<IActionResult> ObtenerBandeja(CancellationToken ct)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (!usuarioId.HasValue)
        {
            return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });
        }

        var notificaciones = await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId.Value)
            .OrderByDescending(n => n.FechaCreacion)
            .ToListAsync(ct);

        return Ok(notificaciones);
    }

    [HttpPatch("{id}/leer")]
    [Authorize]
    public async Task<IActionResult> MarcarComoLeida(long id, CancellationToken ct)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (!usuarioId.HasValue)
        {
            return Unauthorized();
        }

        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == usuarioId.Value, ct);

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
        var usuarioId = ObtenerUsuarioIdActual();

        if (!usuarioId.HasValue)
        {
            var identifierFallback = User.FindFirst(ClaimTypes.Name)?.Value 
                                     ?? User.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(identifierFallback))
            {
                var usuarioDb = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == identifierFallback.ToLower(), ct);

                if (usuarioDb != null)
                {
                    usuarioId = usuarioDb.Id;
                }
            }
        }

        if (!usuarioId.HasValue || usuarioId.Value <= 0)
        {
            return Unauthorized(new { mensaje = "No se pudo identificar al usuario de la sesión para crear el turno." });
        }

        var productoExiste = await _context.Productos.AnyAsync(p => p.Id == dto.ProductoId, ct);
        if (!productoExiste)
        {
            return BadRequest(new { mensaje = "El servicio o producto seleccionado no es válido." });
        }

        DateTime fechaHoraFinal = dto.FechaHora;
        if (!string.IsNullOrWhiteSpace(dto.Horario))
        {
            try 
            {
                var horaInicioStr = dto.Horario.Split('-')[0].Trim(); 
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
            UsuarioId = (int)usuarioId.Value,
            ProductoId = (int)dto.ProductoId,
            BarberoId = dto.BarberoId > 0 ? (int?)dto.BarberoId : null,
            FechaHora = fechaHoraFinal,
            Estado = "Reservado",
            Activo = true
        };

        _context.Turnos.Add(turno);

        try
        {
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
        }
        catch
        {
            // Evitamos que falle la creación si hay un error con las notificaciones
        }
        
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al guardar el turno en la base de datos.", detalle = ex.InnerException?.Message ?? ex.Message });
        }

        var qrUrl = ObtenerUrlDeepLink(turno.TokenPublico);

        return Ok(new { 
            mensaje = "Turno reservado con éxito", 
            turnoId = turno.Id,
            tokenPublico = turno.TokenPublico,
            qrUrl = qrUrl
        });
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
        var usuarioId = ObtenerUsuarioIdActual();

        bool esStaff = rolClaim.Contains("admin") || rolClaim.Contains("barbero");

        if (!esStaff && usuarioId.HasValue)
        {
            if (turno.UsuarioId != usuarioId.Value)
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
        try
        {
            var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.Id == id, ct);

            if (turno == null)
            {
                return NotFound(new { mensaje = $"Turno con ID {id} no encontrado para generar QR." });
            }

            string urlDeepLink = ObtenerUrlDeepLink(turno.TokenPublico);

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(urlDeepLink, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);
                    return File(qrCodeBytes, "image/png");
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno al generar el QR", detalle = ex.Message });
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

    private long? ObtenerUsuarioIdActual()
    {
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                            ?? User.FindFirst("sub")?.Value 
                            ?? User.FindFirst("id")?.Value 
                            ?? User.FindFirst("uid")?.Value;

        if (long.TryParse(usuarioIdStr, out var id))
        {
            return id;
        }
        return null;
    }

 private string ObtenerUrlDeepLink(string tokenPublico)
    {
        var configurada = _configuration["Publico:BaseUrl"];
        var baseUrl = string.IsNullOrWhiteSpace(configurada)
            ? $"{Request.Scheme}://{Request.Host}"
            : configurada.TrimEnd('/');
        
        return $"{baseUrl}/api/publico/turnos/{tokenPublico}";
    }
}

public class TurnoDto
{
    public long ProductoId { get; set; }
    public long BarberoId { get; set; }
    public string Horario { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
}