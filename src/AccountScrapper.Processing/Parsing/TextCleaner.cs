using System.Text;
using System.Text.RegularExpressions;

namespace AccountScrapper.Processing.Parsing;

/// <summary>Cleans text scraped from right-to-left (Hebrew) pages.</summary>
public static partial class TextCleaner
{
    /// <summary>Removes invisible bidi control characters (LRM, RLM, embeddings, isolates) that RTL pages inject.</summary>
    public static string RemoveDirectionalMarks(string text) => BidiMarks().Replace(text, string.Empty);

    /// <summary>Removes bidi marks, collapses whitespace runs and trims.</summary>
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var withoutMarks = RemoveDirectionalMarks(text).Normalize(NormalizationForm.FormC);
        return Whitespace().Replace(withoutMarks, " ").Trim();
    }

    [GeneratedRegex("[‎‏‪-‮⁦-⁩]")]
    private static partial Regex BidiMarks();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
