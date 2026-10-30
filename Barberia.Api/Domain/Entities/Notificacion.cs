using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Barberia.Api.Domain.Entities
{
    [Table("Notificaciones")]
    public class Notificacion : EntityBase
    {
        public int UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Leida { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}