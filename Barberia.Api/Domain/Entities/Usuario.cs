using System.ComponentModel.DataAnnotations.Schema;

namespace Barberia.Api.Domain.Entities
{
    [Table("Usuarios")]
    public class Usuario : EntityBase
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "Cliente"; 
    }
}