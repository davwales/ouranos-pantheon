using System.Globalization;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// Compact number formatting for assistant prompts. Small models tokenize numbers almost digit by
/// digit, so abbreviated values keep prompts within the limited context window.
/// </summary>
public static class AssistantNumberFormat
{
    public static string Compact(decimal value)
    {
        var magnitude = Math.Abs(value);

        return magnitude switch
        {
            >= 1_000_000_000_000m => Number(value / 1_000_000_000_000m) + "T",
            >= 1_000_000_000m => Number(value / 1_000_000_000m) + "B",
            >= 1_000_000m => Number(value / 1_000_000m) + "M",
            >= 10_000m => Number(value / 1_000m, "0.#") + "K",
            _ => Number(value),
        };
    }

    public static string Percent(decimal ratio)
    {
        return Number(ratio * 100, "0.#") + "%";
    }

    public static string Number(decimal value, string format = "0.##")
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }
}
