namespace Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

public static class StringExtensions
{
    private static readonly char[] DanglingTrailingCharacters =
    [
        ',',
        ';',
        ':',
        '-',
        '–',
        '—',
        '(',
        '[',
        '{',
        '&',
        '/',
        '\\',
        '|',
        '+',
    ];

    /// <summary>
    /// Cuts the value to <paramref name="maxLength"/> characters and marks the cut with an
    /// ellipsis; a value that fits is returned unchanged.
    /// </summary>
    public static string TruncateWithEllipsis(this string value, int maxLength)
    {
        return value.Length <= maxLength ? value : $"{value[..maxLength]}…";
    }

    /// <summary>
    /// Cuts the value to at most <paramref name="maxLength"/> characters on a word boundary,
    /// dropping an unclosed <c>&lt;…&gt;</c> tag and any separators left dangling at the end; a
    /// value that fits is returned unchanged.
    /// </summary>
    public static string TruncateAtWordBoundary(this string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        var cut = value[..maxLength];
        if (!char.IsWhiteSpace(value[maxLength]))
        {
            var boundary = cut.Length - 1;
            while (boundary > 0 && !char.IsWhiteSpace(cut[boundary]))
            {
                boundary--;
            }

            // A single word longer than the limit has no boundary, so it is cut mid-word.
            if (boundary > 0)
            {
                cut = cut[..boundary];
            }
        }

        var openTag = cut.LastIndexOf('<');
        if (openTag >= 0 && cut.IndexOf('>', openTag) < 0)
        {
            cut = cut[..openTag];
        }

        string previous;
        do
        {
            previous = cut;
            cut = cut.TrimEnd().TrimEnd(DanglingTrailingCharacters);
        } while (cut.Length != previous.Length);

        return cut;
    }
}
