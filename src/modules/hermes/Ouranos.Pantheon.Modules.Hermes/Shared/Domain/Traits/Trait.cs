using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

public class Trait : BaseEntity<Id<Trait>>
{
    protected Trait(Id<Trait> id) : base(id)
    {
        Name = string.Empty;
        Content = string.Empty;
    }

    public string Name { get; private set; }

    public string Content { get; private set; }

    public bool IsPublic { get; private set; }

    public static Trait Create(
        Id<Trait> id,
        string name,
        string content,
        bool isPublic = true
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(content);

        return new Trait(id)
        {
            Name = name,
            Content = content,
            IsPublic = isPublic
        };
    }

    public void Update(
        string name,
        string content,
        bool isPublic = true
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(content);

        Name = name;
        Content = content;
        IsPublic = isPublic;
    }
}
