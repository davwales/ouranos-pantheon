namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;

/// <summary>
/// A validated condition in the explorer's terms: the column id, the backend filter operator and
/// a normalized value (ROI as a fraction).
/// </summary>
public sealed record ExplorerCondition(string Field, string Operator, string Value);
