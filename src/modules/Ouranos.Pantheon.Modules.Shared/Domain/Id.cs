using System.Diagnostics.CodeAnalysis;

namespace Ouranos.Pantheon.Modules.Shared.Domain;

public readonly record struct Id<T>(string Value) : IParsable<Id<T>>
{
    public static explicit operator string(Id<T> id)
    {
        return id.Value;
    }

    public static explicit operator Id<T>(string value)
    {
        return new Id<T>(value);
    }

    public override string ToString()
    {
        return Value;
    }

    public static Id<T> Parse(string s, IFormatProvider? _)
    {
        return new Id<T>(s);
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? _,
        out Id<T> result
    )
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }

        result = new Id<T>(s);
        return true;
    }
}