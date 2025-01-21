using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Core.Domain.Common;

public abstract class BaseEntity<TId>
{
    public BaseEntity(TId id)
    {
        Guard.Against.Null(id);

        Id = id;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public TId Id { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    protected void Update()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}