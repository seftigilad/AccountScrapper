using System.Globalization;
using AccountScrapper.Core.Models;

namespace AccountScrapper.Processing.Parsing;

/// <summary>
/// Parses amounts as displayed by Israeli financial sites, e.g. "1,234.50", "₪ -80.00",
/// "120.00-" (trailing minus) or "(120.00)" (accounting negative).
/// </summary>
public static class AmountParser
{
    private static readonly string[] CurrencyTokens = ["₪", "ILS", "NIS", "ש\"ח", "ש״ח", "$", "USD", "€", "EUR"];

    public static bool TryParse(string? text, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var cleaned = TextCleaner.RemoveDirectionalMarks(text);
        foreach (var token in CurrencyTokens)
        {
            cleaned = cleaned.Replace(token, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        cleaned = cleaned.Replace(" ", string.Empty, StringComparison.Ordinal)
                         .Replace(" ", string.Empty, StringComparison.Ordinal)
                         .Replace("−", "-", StringComparison.Ordinal); // Unicode minus sign

        var negative = false;
        if (cleaned.StartsWith('(') && cleaned.EndsWith(')'))
        {
            negative = true;
            cleaned = cleaned[1..^1];
        }

        if (cleaned.EndsWith('-'))
        {
            negative = !negative;
            cleaned = cleaned[..^1];
        }

        if (!decimal.TryParse(cleaned, NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        amount = negative ? -value : value;
        return true;
    }

    /// <summary>
    /// Applies an explicit credit/debit indication. Sources that report direction separately
    /// usually show the absolute value, so the direction wins over the textual sign.
    /// </summary>
    public static decimal ApplyDirection(decimal amount, TransactionDirection direction) => direction switch
    {
        TransactionDirection.Credit => Math.Abs(amount),
        TransactionDirection.Debit => -Math.Abs(amount),
        _ => amount,
    };
}
