namespace Barberia.Api.Domain.Entities;

public class Turno : EntityBase
{
    public long UsuarioId { get; set; }
    public long ProductoId { get; set; }
    public long? BarberoId { get; set; } 
    public DateTime FechaHora { get; set; }
    public string Estado { get; set; } = "Reservado"; 
    public bool Activo { get; set; } = true;

    // Token público único de 16 bytes para acceso anónimo
    public string TokenPublico { get; set; } = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();

    // Propiedades de navegación
    public virtual Producto? Producto { get; set; }
    public virtual Usuario? Usuario { get; set; }
    public virtual Usuario? Barbero { get; set; } 
}