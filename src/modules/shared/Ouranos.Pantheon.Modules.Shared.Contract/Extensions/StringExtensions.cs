namespace Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Cuts the value to <paramref name="maxLength"/> characters and marks the cut with an
    /// ellipsis; a value that fits is returned unchanged.
    /// </summary>
    public static string TruncateWithEllipsis(this string value, int maxLength)
    {
        return value.Length <= maxLength ? value : $"{value[..maxLength]}…";
    }
}
