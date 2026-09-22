namespace Barberia.Api.Domain.Entities;

public sealed class Categoria : EntityBase
{
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}