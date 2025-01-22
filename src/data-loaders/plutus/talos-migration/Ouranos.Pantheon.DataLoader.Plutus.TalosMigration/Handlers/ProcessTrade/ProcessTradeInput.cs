using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Handlers.ProcessTrade;

public sealed record ProcessTradeInput(TalosTrade? Trade) : ICommand<ProcessTradeResponse>;