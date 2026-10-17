using System.Text.Json.Serialization;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;

/// <summary>
/// One ordered piece of a saved assistant reply, shared by every conversation slice that reads or
/// writes messages.
/// </summary>
[JsonDerivedType(typeof(ReasoningMessagePartDto), "reasoning")]
[JsonDerivedType(typeof(TextMessagePartDto), "text")]
[JsonDerivedType(typeof(ToolCallMessagePartDto), "toolCall")]
public abstract record MessagePartDto;
