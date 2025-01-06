using Ouranos.Pantheon.Service.Aphrodite.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Aphrodite.Infra.OuranosMl.Requests;

public sealed record GenerateCompletionRequest(
    List<Message> Messages
);