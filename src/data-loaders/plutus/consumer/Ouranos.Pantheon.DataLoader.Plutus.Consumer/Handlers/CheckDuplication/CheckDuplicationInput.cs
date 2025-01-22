using Ouranos.Pantheon.Core.Application.Mediator;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.CheckDuplication;

public sealed record CheckDuplicationInput(Guid MessageId) : IQuery<CheckDuplicationResponse>;