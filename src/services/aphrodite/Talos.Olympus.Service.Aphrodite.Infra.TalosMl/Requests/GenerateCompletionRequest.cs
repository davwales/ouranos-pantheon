using Talos.Olympus.Service.Aphrodite.Domain.Conversations;

namespace Talos.Olympus.Service.Aphrodite.Infra.TalosMl.Requests;

public sealed record GenerateCompletionRequest(
    List<Message> Messages
);