using System.Globalization;

namespace AccountScrapper.Processing.Parsing;

/// <summary>Parses the date formats used by Israeli bank and credit-card sites.</summary>
public static class DateParser
{
    private static readonly string[] Formats =
    [
        "yyyyMMdd",     // Hapoalim API: 20261003
        "dd/MM/yyyy",   // Most bank UIs: 03/10/2026
        "d/M/yyyy",
        "dd/MM/yy",     // Short year: 03/10/26
        "d/M/yy",
        "dd.MM.yyyy",   // Credit card sites: 03.10.2026
        "dd.MM.yy",
        "yyyy-MM-dd",   // ISO
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
    ];

    public static bool TryParse(string? text, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var cleaned = TextCleaner.RemoveDirectionalMarks(text).Trim();

        if (DateTime.TryParseExact(cleaned, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            date = DateOnly.FromDateTime(dateTime);
            return true;
        }

        // Full ISO-8601 with offset / 'Z' (e.g. "2026-10-03T00:00:00+03:00").
        if (DateTimeOffset.TryParse(cleaned, CultureInfo.InvariantCulture, DateTimeStyles.None, out var withOffset))
        {
            date = DateOnly.FromDateTime(withOffset.DateTime);
            return true;
        }

        return false;
    }
}
