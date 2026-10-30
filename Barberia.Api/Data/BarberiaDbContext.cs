using Barberia.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Api.Data;

public class BarberiaDbContext : DbContext
{
    public BarberiaDbContext(DbContextOptions<BarberiaDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Turno> Turnos { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.Precio).HasPrecision(18, 2);
        });
    }
}