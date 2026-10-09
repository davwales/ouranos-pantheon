using System.Text.Json.Serialization;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

[JsonDerivedType(typeof(AssistantReasoningEvent), "reasoning")]
[JsonDerivedType(typeof(AssistantContentEvent), "content")]
[JsonDerivedType(typeof(AssistantUsageEvent), "usage")]
[JsonDerivedType(typeof(AssistantErrorEvent), "error")]
[JsonDerivedType(typeof(AssistantDoneEvent), "done")]
public abstract record AssistantEvent;
