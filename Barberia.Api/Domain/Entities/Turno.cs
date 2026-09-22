namespace Barberia.Api.Domain.Entities;

public class Turno : EntityBase
{
    public long UsuarioId { get; set; }
    public long ProductoId { get; set; }
    public DateTime FechaHora { get; set; }
    public string Estado { get; set; } = "Reservado"; 
    public bool Activo { get; set; } = true;

    // Propiedad de navegación
    public virtual Producto? Producto { get; set; }
}