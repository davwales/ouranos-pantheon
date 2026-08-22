using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Domain;

public abstract class BaseEntity<TId>
{
    protected BaseEntity(TId id)
    {
        Guard.Against.Null(id);

        Id = id;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public TId Id { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; private set; }

    protected void Update()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
