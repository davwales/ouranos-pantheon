using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// What a data tool returns: either its data, or a note telling the model why there is none
/// (for example an unknown id), so the model can recover instead of guessing.
/// </summary>
public sealed record AssistantData<TData>
    where TData : class
{
    private AssistantData(TData? data, string? note)
    {
        Data = data;
        Note = note;
    }

    public TData? Data { get; }

    public string? Note { get; }

    public static implicit operator AssistantData<TData>(TData data)
    {
        return new AssistantData<TData>(Guard.Against.Null(data), null);
    }

    public static AssistantData<TData> WithNote(string note)
    {
        return new AssistantData<TData>(null, Guard.Against.NullOrWhiteSpace(note));
    }
}
