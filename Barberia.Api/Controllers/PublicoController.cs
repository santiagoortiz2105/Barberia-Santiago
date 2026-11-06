using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Barberia.Api.Data;

namespace Barberia.Api.Controllers;

[ApiController]
[Route("api/publico")]
public class PublicoController : ControllerBase
{
    private readonly BarberiaDbContext _context;
    private readonly IConfiguration _configuration;

    public PublicoController(BarberiaDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("turnos/{tokenPublico}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerTurnoPublico(string tokenPublico, CancellationToken cancellationToken)
    {
        // Validamos que el token tenga el formato hexadecimal esperado 
        if (string.IsNullOrWhiteSpace(tokenPublico) || tokenPublico.Length != 32)
        {
            return BadRequest(GenerarHtmlError("Token público inválido."));
        }

        // Buscamos el turno por su token público único y traemos sus relaciones (Producto, Barbero y Usuario)
        var turno = await _context.Turnos
            .Include(t => t.Producto)
            .Include(t => t.Barbero)
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenPublico == tokenPublico, cancellationToken);

        if (turno == null)
        {
            return NotFound(GenerarHtmlError("El turno solicitado no existe o el enlace es incorrecto."));
        }

        string servicioNombre = turno.Producto != null ? turno.Producto.Nombre : "Corte de Cabello";
        decimal precio = turno.Producto != null ? turno.Producto.Precio : 0;
        string barberoNombre = turno.Barbero != null ? turno.Barbero.Nombre : "Profesional asignado";
        string clienteNombre = turno.Usuario != null ? turno.Usuario.Nombre : "Cliente";
        string fechaFormateada = turno.FechaHora.ToString("dd/MM/yyyy HH:mm");

        // HTML moderno y sumamente estético adaptado para dispositivos móviles
        string html = $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Comprobante de Turno - Barbería Santiago</title>
            <style>
                * {{ box-sizing: border-box; margin: 0; padding: 0; font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; }}
                body {{
                    background-color: #0f172a;
                    color: #f8fafc;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    min-height: 100vh;
                    padding: 16px;
                }}
                .card {{
                    background: #1e293b;
                    width: 100%;
                    max-width: 420px;
                    border-radius: 24px;
                    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.3), 0 10px 10px -5px rgba(0, 0, 0, 0.2);
                    overflow: hidden;
                    border: 1px solid #334155;
                }}
                .header {{
                    background: linear-gradient(135deg, #3b82f6, #1d4ed8);
                    color: white;
                    padding: 30px 20px;
                    text-align: center;
                    position: relative;
                }}
                .header h1 {{ font-size: 1.5rem; font-weight: 800; margin-bottom: 6px; letter-spacing: -0.5px; }}
                .header p {{ font-size: 0.85rem; opacity: 0.9; font-weight: 400; }}
                .body-content {{
                    padding: 24px 20px;
                }}
                .row {{
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    margin-bottom: 16px;
                    padding-bottom: 14px;
                    border-bottom: 1px solid #334155;
                }}
                .row:last-child {{ border-bottom: none; margin-bottom: 0; padding-bottom: 0; }}
                .label {{ font-size: 0.85rem; color: #94a3b8; font-weight: 500; }}
                .value {{ font-size: 0.95rem; color: #f1f5f9; font-weight: 600; text-align: right; }}
                .badge-estado {{
                    background: rgba(34, 197, 94, 0.15);
                    color: #4ade80;
                    padding: 6px 14px;
                    border-radius: 30px;
                    font-size: 0.75rem;
                    font-weight: 700;
                    text-transform: uppercase;
                    letter-spacing: 0.5px;
                }}
                .price {{
                    color: #60a5fa;
                    font-size: 1.25rem;
                    font-weight: 800;
                }}
                .footer {{
                    background: #172033;
                    text-align: center;
                    padding: 16px;
                    font-size: 0.75rem;
                    color: #64748b;
                    border-top: 1px solid #334155;
                }}
            </style>
        </head>
        <body>
            <div class='card'>
                <div class='header'>
                    <h1>💈 Barbería Santiago</h1>
                    <p>Comprobante de Turno Oficial</p>
                </div>
                <div class='body-content'>
                    <div class='row'>
                        <span class='label'>Estado</span>
                        <span class='badge-estado'>{System.Net.WebUtility.HtmlEncode(turno.Estado)}</span>
                    </div>
                    <div class='row'>
                        <span class='label'>Servicio</span>
                        <span class='value'>{System.Net.WebUtility.HtmlEncode(servicioNombre)}</span>
                    </div>
                    <div class='row'>
                        <span class='label'>Fecha y Hora</span>
                        <span class='value'>{fechaFormateada} hs</span>
                    </div>
                    <div class='row'>
                        <span class='label'>Cliente</span>
                        <span class='value'>{System.Net.WebUtility.HtmlEncode(clienteNombre)}</span>
                    </div>
                    <div class='row'>
                        <span class='label'>Barbero</span>
                        <span class='value'>{System.Net.WebUtility.HtmlEncode(barberoNombre)}</span>
                    </div>
                    <div class='row' style='margin-top: 12px;'>
                        <span class='label'>Precio Total</span>
                        <span class='value price'>${precio:N0}</span>
                    </div>
                </div>
                <div class='footer'>
                    Presenta este comprobante en el local al llegar a tu turno.
                </div>
            </div>
        </body>
        </html>";

        return Content(html, "text/html");
    }

    private ContentResult GenerarHtmlError(string mensaje)
    {
        string htmlError = $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Error - Barbería Santiago</title>
            <style>
                body {{ background-color: #0f172a; color: #f8fafc; font-family: sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; text-align: center; padding: 20px; }}
                .error-box {{ background: #1e293b; padding: 30px; border-radius: 16px; border: 1px solid #334155; max-width: 360px; }}
                h2 {{ color: #f87171; font-size: 1.2rem; margin-bottom: 10px; }}
                p {{ color: #94a3b8; font-size: 0.9rem; }}
            </style>
        </head>
        <body>
            <div class='error-box'>
                <h2>⚠️ Enlace no válido</h2>
                <p>{System.Net.WebUtility.HtmlEncode(mensaje)}</p>
            </div>
        </body>
        </html>";

        return Content(htmlError, "text/html");
    }
}