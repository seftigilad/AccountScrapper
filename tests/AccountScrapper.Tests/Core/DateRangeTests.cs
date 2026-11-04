using System.Globalization;
using AccountScrapper.Core.Models;

namespace AccountScrapper.Tests.Core;

public class DateRangeTests
{
    [Theory]
    [InlineData("2026-11-04", "2026-10-01", "2026-10-31")]
    [InlineData("2026-01-15", "2025-12-01", "2025-12-31")] // crosses a year
    [InlineData("2028-03-01", "2028-02-01", "2028-02-29")] // leap year
    public void PreviousMonth_ReturnsWholePreviousCalendarMonth(string today, string expectedStart, string expectedEnd)
    {
        var range = DateRange.PreviousMonth(DateOnly.Parse(today, CultureInfo.InvariantCulture));

        Assert.Equal(DateOnly.Parse(expectedStart, CultureInfo.InvariantCulture), range.Start);
        Assert.Equal(DateOnly.Parse(expectedEnd, CultureInfo.InvariantCulture), range.End);
    }

    [Fact]
    public void Contains_IsInclusiveOnBothEnds()
    {
        var range = new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.True(range.Contains(new DateOnly(2026, 10, 1)));
        Assert.True(range.Contains(new DateOnly(2026, 10, 31)));
        Assert.False(range.Contains(new DateOnly(2026, 11, 1)));
    }

    [Fact]
    public void Constructor_RejectsInvertedRange() =>
        Assert.Throws<ArgumentException>(() => new DateRange(new DateOnly(2026, 10, 2), new DateOnly(2026, 10, 1)));

    [Fact]
    public void Credentials_ToString_DoesNotRevealValues()
    {
        var text = new BankCredentials("AB1234", "s3cret!").ToString();

        Assert.DoesNotContain("AB1234", text);
        Assert.DoesNotContain("s3cret!", text);
    }
}
