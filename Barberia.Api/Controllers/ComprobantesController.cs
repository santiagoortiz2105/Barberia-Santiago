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
            .Include(t => t.Barbero)
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (turno == null)
        {
            return NotFound(new { mensaje = "El turno no fue encontrado." });
        }

        var servicio = turno.Producto != null ? turno.Producto.Nombre : "Corte de Cabello";
        var precioValor = turno.Producto != null ? turno.Producto.Precio : 0;
        var precio = $"${precioValor:N0}";
        var fecha = turno.FechaHora.ToString("dd/MM/yyyy HH:mm");
        var cliente = turno.Usuario != null ? turno.Usuario.Nombre : $"Usuario #{turno.UsuarioId}";
        var barbero = turno.Barbero != null ? turno.Barbero.Nombre : "Profesional asignado";
        var estado = turno.Estado ?? "Reservado";

        try
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(3, Unit.Centimetre);
                    page.PageColor("#0f172a"); // Fondo oscuro idéntico a la web

                    page.DefaultTextStyle(x => x.FontSize(11).FontColor("#f1f5f9"));

                    page.Content()
                        .AlignCenter()
                        .AlignMiddle()
                        .Column(mainCol =>
                        {
                            mainCol.Item().Width(420).Element(card =>
                            {
                                card.Background("#1e293b")
                                    .Border(1)
                                    .BorderColor("#334155")
                                    .Column(col =>
                                    {
                                        // HEADER ESTILIZADO
                                        col.Item()
                                            .Background("#2563eb")
                                            .Padding(20)
                                            .AlignCenter()
                                            .Column(headerCol =>
                                            {
                                                headerCol.Item().Text("Barbería Santiago").Bold().FontSize(18).FontColor("#ffffff");
                                                headerCol.Item().PaddingTop(4).Text("Comprobante de Turno Oficial").FontSize(10).FontColor("#dbeafe");
                                            });

                                        // CONTENIDO DE LA TARJETA
                                        col.Item().Padding(20).Column(body =>
                                        {
                                            body.Spacing(12);

                                            // Fila Estado
                                            body.Item().Row(row =>
                                            {
                                                row.RelativeItem().Text("Estado").FontColor("#94a3b8").FontSize(10);
                                                row.ConstantItem(100).AlignRight().Text(estado).Bold().FontColor("#4ade80");
                                            });

                                            body.Item().LineHorizontal(1).LineColor("#334155");

                                            // Fila Servicio
                                            body.Item().Row(row =>
                                            {
                                                row.RelativeItem().Text("Servicio").FontColor("#94a3b8");
                                                row.RelativeItem().AlignRight().Text(servicio).Bold();
                                            });

                                            body.Item().LineHorizontal(1).LineColor("#334155");

                                            // Fila Fecha y Hora
                                            body.Item().Row(row =>
                                            {
                                                row.RelativeItem().Text("Fecha y Hora").FontColor("#94a3b8");
                                                row.RelativeItem().AlignRight().Text($"{fecha} hs").Bold();
                                            });

                                            body.Item().LineHorizontal(1).LineColor("#334155");

                                            // Fila Cliente
                                            body.Item().Row(row =>
                                            {
                                                row.RelativeItem().Text("Cliente").FontColor("#94a3b8");
                                                row.RelativeItem().AlignRight().Text(cliente).Bold();
                                            });

                                            body.Item().LineHorizontal(1).LineColor("#334155");

                                            // Fila Barbero
                                            body.Item().Row(row =>
                                            {
                                                row.RelativeItem().Text("Barbero").FontColor("#94a3b8");
                                                row.RelativeItem().AlignRight().Text(barbero).Bold();
                                            });

                                            body.Item().LineHorizontal(1).LineColor("#334155");

                                            // Fila Precio Total
                                            body.Item().PaddingTop(4).Row(row =>
                                            {
                                                row.RelativeItem().Text("Precio Total").FontColor("#94a3b8").SemiBold();
                                                row.RelativeItem().AlignRight().Text(precio).Bold().FontSize(15).FontColor("#60a5fa");
                                            });
                                        });

                                        // FOOTER DE LA TARJETA
                                        col.Item()
                                            .Background("#172033")
                                            .BorderTop(1)
                                            .BorderColor("#334155")
                                            .Padding(12)
                                            .AlignCenter()
                                            .Text("Presenta este comprobante en el local al llegar a tu turno.")
                                            .FontSize(9)
                                            .FontColor("#64748b");
                                    });
                            });
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