using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

public sealed class TradeMigration(Id<TradeMigration> id) : BaseEntity<Id<TradeMigration>>(id), ICommand;