namespace Ouranos.Pantheon.Hermes.Service.Domain.Conversations;

public sealed record Message(
    string Content,
    Role Role
);