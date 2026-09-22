using Barberia.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Barberia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComprobantesController : ControllerBase
{
    private readonly BarberiaDbContext _context;

    public ComprobantesController(BarberiaDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [HttpGet("turno/{id}/pdf")]
    public async Task<IActionResult> GenerarComprobanteTurno(long id, CancellationToken ct)
    {
        var turno = await _context.Turnos
            .Include(t => t.Producto)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (turno == null)
        {
            return NotFound(new { mensaje = "El turno no fue encontrado." });
        }

        var servicio = turno.Producto != null ? turno.Producto.Nombre : "Servicio general";
        var precioValor = turno.Producto != null ? turno.Producto.Precio : 0;
        var precio = $"${precioValor:N0}";
        var fecha = turno.FechaHora.ToString("dd/MM/yyyy HH:mm");
        var cliente = $"Usuario #{turno.UsuarioId}";

        try
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    
                    // Se remueve .FontFamily("Arial") para usar la fuente por defecto (Lato)
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("BARBERÍA SANTIAGO - COMPROBANTE")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Spacing(10);
                            col.Item().Text($"Comprobante Nro: #{id}").Bold();
                            col.Item().Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}");
                            col.Item().Text($"Fecha del Turno: {fecha}");
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            
                            col.Item().Text($"Cliente: {cliente}");
                            col.Item().Text($"Servicio realizado: {servicio}");
                            col.Item().Text($"Total abonado: {precio}").Bold().FontSize(14);
                            col.Item().Text($"Estado: {turno.Estado}");
                            
                            col.Item().PaddingTop(20).Text("¡Gracias por confiar en nuestros servicios!").Italic();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                });
            });

            byte[] pdfBytes = documento.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"comprobante_turno_{id}.pdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n----------------- ERROR EN COMPROBANTE -----------------");
            Console.WriteLine($"Mensaje: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            Console.WriteLine($"---------------------------------------------------------\n\n");

            return StatusCode(500, new { mensaje = ex.Message, detalle = ex.ToString() });
        }
    }
}