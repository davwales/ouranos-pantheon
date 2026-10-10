namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// Output guidance appended to every assistant's instructions, because every assistant renders in
/// the same narrow chat panel.
/// </summary>
public static class AssistantFormatting
{
    public const string Instructions = """
        Your reply is shown in a narrow chat panel. Lead with the answer in a sentence or two, then
        add supporting detail. Prefer short bullet lists. Use a markdown table only for a small
        comparison of at most three short columns. Use bold for key numbers and use headings
        sparingly, no larger than ###. Never use LaTeX or math notation: write calculations inline,
        for example 195 × 0.98 = 191.1. Do not use horizontal rules.
        """;
}
