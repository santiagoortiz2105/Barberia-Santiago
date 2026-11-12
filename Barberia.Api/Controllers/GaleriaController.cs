using Barberia.Api.Data;
using Barberia.Api.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class GaleriaController : ControllerBase
{
    private readonly BarberiaDbContext _context;

    public GaleriaController(BarberiaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerGaleria()
    {
        // Obtiene todas las imágenes de la base de datos, de la más nueva a la más vieja
        var imagenes = await _context.GaleriaItems
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(imagenes);
    }

    [HttpPost]
    public async Task<IActionResult> SubirImagen([FromBody] ImagenDto dto)
    {
        if (string.IsNullOrEmpty(dto.ImagenBase64))
        {
            return BadRequest(new { mensaje = "La imagen es obligatoria" });
        }

        var nuevaImagen = new GaleriaItem
        {
            Url = dto.ImagenBase64
        };

        // Guarda la imagen en MySQL
        _context.GaleriaItems.Add(nuevaImagen);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, mensaje = "Imagen subida con éxito", id = nuevaImagen.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarImagen(int id)
    {
        var imagen = await _context.GaleriaItems.FindAsync(id);
        if (imagen == null)
        {
            return NotFound(new { mensaje = "Imagen no encontrada" });
        }

        // Elimina la imagen de la base de datos
        _context.GaleriaItems.Remove(imagen);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, mensaje = "Imagen eliminada con éxito" });
    }
}

public class ImagenDto
{
    public string ImagenBase64 { get; set; }
}