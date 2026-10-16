namespace Ouranos.Pantheon.Modules.Plutus.Shared;

// Rows and points are capped because tool output shares the inference model's limited context
// window with the prompt, the conversation and the reply.
public sealed record AssistantToolOptions(
    int ChartPoints,
    int MaxPositions,
    int DefaultRows,
    int MaxRows
)
{
    public AssistantToolOptions()
        : this(ChartPoints: 8, MaxPositions: 10, DefaultRows: 10, MaxRows: 12) { }

    public int ClampRows(int? requested)
    {
        return Math.Clamp(requested ?? DefaultRows, 1, MaxRows);
    }
}
