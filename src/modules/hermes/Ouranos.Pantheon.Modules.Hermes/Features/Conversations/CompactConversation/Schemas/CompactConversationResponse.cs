using System.Text.Json.Serialization;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation.Schemas;

[JsonDerivedType(typeof(CompactContentChunkResponse), "content")]
[JsonDerivedType(typeof(CompactUsageChunkResponse), "usage")]
[JsonDerivedType(typeof(CompactCompleteResponse), "complete")]
public abstract record CompactConversationResponse;