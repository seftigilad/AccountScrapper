using System.Text;
using Microsoft.Extensions.Options;

namespace AccountScrapper.Processing.Masking;

/// <summary>
/// Masks descriptions according to <see cref="MaskingOptions.Mode"/>.
/// In <see cref="MaskingMode.Partial"/> mode the shape of the text survives (so it is still possible to tell
/// a salary from a card payment at a glance) while names and numbers become unreadable:
/// "העברה לישראל ישראלי 123456" → "ה**** ל***** י****** ******".
/// </summary>
public sealed class DescriptionMasker : IDescriptionMasker
{
    public const string FullMaskPlaceholder = "***";
    private const char MaskChar = '*';

    private readonly MaskingMode _mode;

    public DescriptionMasker(IOptions<MaskingOptions> options)
    {
        _mode = options.Value.Mode;
    }

    public string Mask(string description)
    {
        if (string.IsNullOrEmpty(description))
        {
            return description;
        }

        return _mode switch
        {
            MaskingMode.None => description,
            MaskingMode.Full => FullMaskPlaceholder,
            MaskingMode.Partial => MaskPartially(description),
            _ => throw new InvalidOperationException($"Unsupported masking mode '{_mode}'."),
        };
    }

    private static string MaskPartially(string text)
    {
        var result = new StringBuilder(text.Length);
        var atWordStart = true;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsDigit(c))
            {
                // Numbers are the most sensitive part (accounts, cards, references) - never reveal a digit.
                result.Append(MaskChar);
                atWordStart = false;
            }
            else if (char.IsLetter(c))
            {
                result.Append(atWordStart ? c : MaskChar);
                atWordStart = false;
            }
            else if (IsInWordMark(c) && !atWordStart && i + 1 < text.Length && char.IsLetter(text[i + 1]))
            {
                // Hebrew abbreviations put quotes inside a word (בע"מ, מאוצה"ח, ח'): mask the mark too and
                // stay in the same word, otherwise the letter after it would be revealed.
                result.Append(MaskChar);
            }
            else
            {
                // Whitespace and punctuation are kept and mark the start of a new word.
                result.Append(c);
                atWordStart = true;
            }
        }

        return result.ToString();
    }

    private static bool IsInWordMark(char c) => c is '"' or '\'' or '״' or '׳';
}
