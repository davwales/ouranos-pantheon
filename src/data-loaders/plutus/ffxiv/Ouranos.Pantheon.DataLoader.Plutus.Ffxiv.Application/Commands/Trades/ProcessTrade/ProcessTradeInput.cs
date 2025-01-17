using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Commands.Trades.ProcessTrade;

public sealed record ProcessTradeInput(
    string ItemCode,
    IReadOnlyCollection<ProcessTradeSaleInput> Sales
) : ICommand;