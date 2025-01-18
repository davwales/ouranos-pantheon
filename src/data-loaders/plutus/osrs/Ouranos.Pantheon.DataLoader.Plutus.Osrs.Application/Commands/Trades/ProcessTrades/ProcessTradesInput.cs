using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Commands.Trades.ProcessTrades;

public sealed record ProcessTradesInput(List<GetTradesResponse> Trades) : ICommand;