using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

/// <summary>Reconstructed signals per decision day and symbol.</summary>
public sealed class SwingSignalTable(
    IReadOnlyDictionary<DateOnly, IReadOnlyDictionary<Id<Symbol>, SwingSignalDay>> days
)
{
    private static readonly IReadOnlyDictionary<Id<Symbol>, SwingSignalDay> Empty =
        new Dictionary<Id<Symbol>, SwingSignalDay>();

    public IReadOnlyDictionary<Id<Symbol>, SwingSignalDay> On(DateOnly day)
    {
        return days.TryGetValue(day, out var symbols) ? symbols : Empty;
    }
}
