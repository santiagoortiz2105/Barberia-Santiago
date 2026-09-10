namespace Barberia.Api.Domain.Entities;

public abstract class EntityBase
{
    public long Id { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
