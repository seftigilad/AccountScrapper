namespace AccountScrapper.Core.Models;

/// <summary>An inclusive range of dates.</summary>
public sealed record DateRange
{
    public DateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            throw new ArgumentException($"Range end ({end:yyyy-MM-dd}) is before its start ({start:yyyy-MM-dd}).", nameof(end));
        }

        Start = start;
        End = end;
    }

    public DateOnly Start { get; }

    public DateOnly End { get; }

    public bool Contains(DateOnly date) => date >= Start && date <= End;

    /// <summary>The full calendar month preceding the month of <paramref name="today"/>.</summary>
    public static DateRange PreviousMonth(DateOnly today)
    {
        var firstOfThisMonth = new DateOnly(today.Year, today.Month, 1);
        var start = firstOfThisMonth.AddMonths(-1);
        return new DateRange(start, firstOfThisMonth.AddDays(-1));
    }

    public override string ToString() => $"{Start:yyyy-MM-dd}..{End:yyyy-MM-dd}";
}
