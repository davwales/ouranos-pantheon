using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Service.Aphrodite.Domain.Characters;

public sealed class Character : BaseEntity<Id<Character>>
{
    public Character(
        Id<Character> id,
        string name,
        int age,
        List<CharacterDetail> details
    ) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Age = age;
        Details = details ?? [];
    }

    public string Name { get; private set; }

    public int Age { get; private set; }

    public List<CharacterDetail> Details { get; private set; }

    public void Update(
        string name,
        int age,
        List<CharacterDetail> details
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Update();
        Name = name;
        Age = age;
        Details = details ?? [];
    }
}