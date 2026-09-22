using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Barberia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComprobantesController : ControllerBase
{
    public ComprobantesController()
    {
  
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [HttpGet("turno/{id}/pdf")]
    public IActionResult GenerarComprobanteTurno(long id)
    {

        var cliente = "Santiago Ortiz";
        var servicio = "Corte Clásico + Perfilado";
        var precio = "$12.100";
        var fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // Diseño del PDF con QuestPDF
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Helvetica"));

                // Encabezado
                page.Header()
                    .Text("BARBERÍA SANTIAGO - COMPROBANTE")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                // Contenido principal
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Comprobante Nro: #{id}").Bold();
                        col.Item().Text($"Fecha de emisión: {fecha}");
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        
                        col.Item().Text($"Cliente: {cliente}");
                        col.Item().Text($"Servicio realizado: {servicio}");
                        col.Item().Text($"Total abonado: {precio}").Bold().FontSize(14);
                        
                        col.Item().PaddingTop(20).Text("¡Gracias por confiar en nuestros servicios!").Italic();
                    });

                // Pie de página
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
}