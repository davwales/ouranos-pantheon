namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record CompletionToolCallInput(
    string CallId,
    string Name,
    string Arguments,
    string Output
);
