using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Tests.Utils;

public static class AssistantPromptBudget
{
    public const int Characters = 8000;

    public static int Measure<TContext>(
        string systemPrompt,
        IEnumerable<IAssistantTool<TContext>> tools
    )
        where TContext : class
    {
        var definitions = tools.ToList();
        var instructions =
            systemPrompt.Length + "\n\n".Length + AssistantFormatting.Instructions.Length;

        if (definitions.Count == 0)
        {
            return instructions;
        }

        return instructions
            + "\n\n".Length
            + AssistantFormatting.ToolInstructions.Length
            + definitions.Sum(tool =>
                tool.Name.Length + tool.Description.Length + tool.ParametersSchema.ToString().Length
            );
    }
}
