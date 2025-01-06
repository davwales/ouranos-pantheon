namespace Ouranos.Pantheon.Core.Domain.Common;

public abstract class BaseEntity<TId>
{
    public BaseEntity(TId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public TId Id { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; protected set; }

    protected void Update()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}