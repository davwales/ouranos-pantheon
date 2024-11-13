namespace Talos.Olympus.Core.Domain.Common;

public readonly record struct Id<T>(string Value)
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
}