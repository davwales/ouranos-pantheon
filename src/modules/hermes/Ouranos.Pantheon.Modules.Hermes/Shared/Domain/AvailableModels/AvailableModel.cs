using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableModels;

public class AvailableModel : BaseEntity<Id<AvailableModel>>
{
    protected AvailableModel(Id<AvailableModel> id)
        : base(id)
    {
        ModelIdentifier = string.Empty;
        OwnedBy = string.Empty;
    }

    public string ModelIdentifier { get; private set; }

    public string OwnedBy { get; private set; }

    public static AvailableModel Create(
        Id<AvailableModel> id,
        string modelIdentifier,
        string ownedBy
    )
    {
        Guard.Against.NullOrWhiteSpace(modelIdentifier);

        return new AvailableModel(id)
        {
            ModelIdentifier = modelIdentifier,
            OwnedBy = ownedBy ?? string.Empty,
        };
    }

    public void Update(string modelIdentifier, string ownedBy)
    {
        Guard.Against.NullOrWhiteSpace(modelIdentifier);

        ModelIdentifier = modelIdentifier;
        OwnedBy = ownedBy ?? string.Empty;
        Update();
    }
}
