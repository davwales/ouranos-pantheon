using Ouranos.Pantheon.Service.Hermes.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl.Requests;

public sealed record GenerateCompletionRequest(
    List<Message> Messages
);