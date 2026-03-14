namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

public sealed record Message(
    string Content,
    Role Role
);