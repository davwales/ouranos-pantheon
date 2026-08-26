namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

public sealed record InputWeight(InputKind Kind, decimal Weight)
{
    private InputWeight()
        : this(default, 0m) { }
}
