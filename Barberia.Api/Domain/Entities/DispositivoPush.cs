namespace Barberia.Api.Domain.Entities;

public class DispositivoPush : EntityBase
{
    public long UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ActualizadoEn { get; set; }
    public bool Activo { get; set; } = true;
    
    public Usuario? Usuario { get; set; }
}