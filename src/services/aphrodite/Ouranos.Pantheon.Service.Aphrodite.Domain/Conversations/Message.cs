namespace Ouranos.Pantheon.Service.Aphrodite.Domain.Conversations;

public sealed record Message(
    string Content,
    Role Role
);