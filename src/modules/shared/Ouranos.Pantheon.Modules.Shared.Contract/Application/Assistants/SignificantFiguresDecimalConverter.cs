using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// Rounds decimals to a few significant figures: small models tokenize numbers almost digit by
/// digit, and full-precision prices would crowd the limited context window.
/// </summary>
internal sealed class SignificantFiguresDecimalConverter : JsonConverter<decimal>
{
    internal const int SignificantFigures = 4;

    public override decimal Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(Round(value));
    }

    internal static decimal Round(decimal value)
    {
        if (value == 0)
        {
            return 0;
        }

        var magnitude = (int)Math.Floor(Math.Log10((double)Math.Abs(value)));
        var decimals = SignificantFigures - 1 - magnitude;

        if (decimals >= 0)
        {
            var rounded = Math.Round(value, Math.Min(decimals, 28), MidpointRounding.AwayFromZero);

            // Dividing by a high-scale one drops trailing zeros (2.500 → 2.5) without changing
            // the value, so they are not serialized.
            return rounded / 1.000000000000000000000000000m;
        }

        var scale = (decimal)Math.Pow(10, -decimals);
        return Math.Round(value / scale, MidpointRounding.AwayFromZero) * scale;
    }
}
