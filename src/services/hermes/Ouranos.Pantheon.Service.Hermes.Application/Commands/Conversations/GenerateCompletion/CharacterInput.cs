using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;

public sealed record CharacterInput(
    Id<Character> Id,
    string Name,
    int Age,
    List<CharacterDetail> Details
);