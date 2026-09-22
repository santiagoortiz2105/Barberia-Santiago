using Barberia.Api.Data;
using Barberia.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly BarberiaDbContext _context;

    public ProductosController(BarberiaDbContext context)
    {
        _context = context;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen(CancellationToken ct)
    {
        if (!await _context.Categorias.AnyAsync(ct))
        {
            var catCortes = new Categoria { Nombre = "Cortes de Pelo" };
            var catBarba = new Categoria { Nombre = "Barbería y Barba" };

            _context.Categorias.AddRange(catCortes, catBarba);
            await _context.SaveChangesAsync(ct);

            _context.Productos.AddRange(
                new Producto { CategoriaId = catCortes.Id, Nombre = "Corte Clásico", Descripcion = "Corte con tijera y máquina", Precio = 10000, Disponible = true },
                new Producto { CategoriaId = catCortes.Id, Nombre = "Degradado / Fade", Descripcion = "Fade con navaja al ras", Precio = 7000, Disponible = true },
                new Producto { CategoriaId = catBarba.Id, Nombre = "Perfilado de Barba", Descripcion = "Afeitado con toalla caliente", Precio = 2100, Disponible = true }
            );
            await _context.SaveChangesAsync(ct);
        }

        var total = await _context.Productos.CountAsync(x => x.Activo, ct);
        var disponibles = await _context.Productos.CountAsync(x => x.Activo && x.Disponible, ct);

        var categorias = await _context.Categorias
            .Where(c => c.Activo)
            .Select(c => new { c.Id, c.Nombre })
            .ToListAsync(ct);

        var productos = await _context.Productos
            .Where(p => p.Activo)
            .Select(p => new
            {
                p.Id,
                p.CategoriaId,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.ImagenUrl,
                p.Disponible
            })
            .ToListAsync(ct);

        return Ok(new
        {
            resumen = new { total, disponibles },
            categorias,
        productos
        });
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] Producto producto, IFormFile? imagen, CancellationToken ct)
    {
        if (await _context.Productos.AnyAsync(p => p.Activo && p.Nombre == producto.Nombre, ct))
        {
            return BadRequest(new { codigo = "duplicado", mensaje = "Ya existe un servicio con ese nombre" });
        }

        if (imagen != null && imagen.Length > 0)
        {
            var nombreUnico = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
            var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(rutaCarpeta))
            {
                Directory.CreateDirectory(rutaCarpeta);
            }

            var rutaCompleta = Path.Combine(rutaCarpeta, nombreUnico);

            using (var stream = new System.IO.FileStream(rutaCompleta, System.IO.FileMode.Create))
            {
                await imagen.CopyToAsync(stream, ct);
            }

            producto.ImagenUrl = "/uploads/" + nombreUnico;
        }

        producto.Activo = true;
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync(ct);
        return Ok(producto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Modificar(long id, [FromForm] Producto productoDto, IFormFile? imagen, CancellationToken ct)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (producto == null || !producto.Activo)
        {
            return NotFound(new { codigo = "no_encontrado", mensaje = "El servicio no existe" });
        }

        if (await _context.Productos.AnyAsync(p => p.Activo && p.Id != id && p.Nombre == productoDto.Nombre, ct))
        {
            return BadRequest(new { codigo = "duplicado", mensaje = "Ya existe otro servicio con ese nombre" });
        }

        if (imagen != null && imagen.Length > 0)
        {
            var nombreUnico = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
            var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(rutaCarpeta))
            {
                Directory.CreateDirectory(rutaCarpeta);
            }

            var rutaCompleta = Path.Combine(rutaCarpeta, nombreUnico);

            using (var stream = new System.IO.FileStream(rutaCompleta, System.IO.FileMode.Create))
            {
                await imagen.CopyToAsync(stream, ct);
            }

            producto.ImagenUrl = "/uploads/" + nombreUnico;
        }

        producto.Nombre = productoDto.Nombre;
        producto.Descripcion = productoDto.Descripcion;
        producto.Precio = productoDto.Precio;
        producto.CategoriaId = productoDto.CategoriaId;
        producto.Disponible = productoDto.Disponible;

        await _context.SaveChangesAsync(ct);
        return Ok(producto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DarDeBaja(long id, CancellationToken ct)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (producto == null || !producto.Activo)
        {
            return NotFound(new { codigo = "no_encontrado", mensaje = "El servicio no existe" });
        }

        producto.Activo = false;
        await _context.SaveChangesAsync(ct);
        return Ok(new { mensaje = "Servicio dado de baja correctamente" });
    }
}