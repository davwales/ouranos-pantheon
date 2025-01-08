using MediatR;
using Ouranos.Pantheon.Core.Application.Common;

namespace Ouranos.Pantheon.Service.Hermes.Application.Queries.Conversations.GetCompletion;

public sealed record GetCompletionInput(
    ConversationInput Conversation
) : IStreamRequest<Chunk<string>>;