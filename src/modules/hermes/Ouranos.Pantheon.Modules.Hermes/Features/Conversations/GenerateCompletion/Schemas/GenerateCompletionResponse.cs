using System.Text.Json.Serialization;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

[JsonDerivedType(typeof(SystemPromptChunkResponse), "systemPrompt")]
[JsonDerivedType(typeof(ContentChunkResponse), "content")]
[JsonDerivedType(typeof(UsageChunkResponse), "usage")]
public abstract record GenerateCompletionResponse;
