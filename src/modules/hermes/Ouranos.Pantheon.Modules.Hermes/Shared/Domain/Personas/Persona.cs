using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;

public class Persona : BaseEntity<Id<Persona>>
{
    protected Persona(Id<Persona> id)
        : base(id)
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public string? Personality { get; private set; }

    public string? Scenario { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsPublic { get; private set; }

    public static Persona Create(
        Id<Persona> id,
        string name,
        string description,
        string? personality = null,
        string? scenario = null,
        bool isDefault = false,
        bool isPublic = true
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(description);

        return new Persona(id)
        {
            Name = name,
            Description = description,
            Personality = personality,
            Scenario = scenario,
            IsDefault = isDefault,
            IsPublic = isPublic,
        };
    }

    public void Update(
        string name,
        string description,
        string? personality = null,
        string? scenario = null,
        bool isDefault = false,
        bool isPublic = true
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(description);

        Name = name;
        Description = description;
        Personality = personality;
        Scenario = scenario;
        IsDefault = isDefault;
        IsPublic = isPublic;
    }
}
