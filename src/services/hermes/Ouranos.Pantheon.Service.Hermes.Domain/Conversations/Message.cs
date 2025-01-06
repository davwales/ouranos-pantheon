namespace Ouranos.Pantheon.Service.Hermes.Domain.Conversations;

public sealed record Message(
    string Content,
    Role Role
);