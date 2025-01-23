using MongoDB.Bson;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

public sealed class TradeMigration(ObjectId id) : BaseEntity<ObjectId>(id), ICommand;