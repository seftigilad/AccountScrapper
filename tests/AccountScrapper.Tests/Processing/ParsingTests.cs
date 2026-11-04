using AccountScrapper.Core.Models;
using AccountScrapper.Processing.Parsing;

namespace AccountScrapper.Tests.Processing;

public class ParsingTests
{
    [Theory]
    [InlineData("20261003")]
    [InlineData("03/10/2026")]
    [InlineData("3/10/2026")]
    [InlineData("03/10/26")]
    [InlineData("03.10.2026")]
    [InlineData("03.10.26")]
    [InlineData("2026-10-03")]
    [InlineData("2026-10-03T00:00:00")]
    [InlineData(" ‏03/10/2026 ")] // RTL mark + padding, as scraped from Hebrew pages
    public void DateParser_ParsesSupportedFormats(string text)
    {
        Assert.True(DateParser.TryParse(text, out var date));
        Assert.Equal(new DateOnly(2026, 10, 3), date);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a date")]
    [InlineData("32/10/2026")]
    public void DateParser_RejectsInvalidInput(string text) =>
        Assert.False(DateParser.TryParse(text, out _));

    [Theory]
    [InlineData("1234.5", 1234.5)]
    [InlineData("1,234.50", 1234.50)]
    [InlineData("₪ 1,234.50", 1234.50)]
    [InlineData("-80.00", -80.00)]
    [InlineData("₪ -80.00", -80.00)]
    [InlineData("80.00-", -80.00)]      // trailing minus
    [InlineData("(80.00)", -80.00)]     // accounting negative
    [InlineData("‏80.00 ש\"ח", 80.00)]
    [InlineData("−80.00", -80.00)] // unicode minus sign
    public void AmountParser_ParsesSupportedFormats(string text, double expected)
    {
        Assert.True(AmountParser.TryParse(text, out var amount));
        Assert.Equal((decimal)expected, amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1.2.3")]
    public void AmountParser_RejectsInvalidInput(string text) =>
        Assert.False(AmountParser.TryParse(text, out _));

    [Theory]
    [InlineData(100, TransactionDirection.Credit, 100)]
    [InlineData(-100, TransactionDirection.Credit, 100)]
    [InlineData(100, TransactionDirection.Debit, -100)]
    [InlineData(100, TransactionDirection.Unknown, 100)]
    [InlineData(-100, TransactionDirection.Unknown, -100)]
    public void ApplyDirection_DirectionOverridesTextualSign(decimal amount, TransactionDirection direction, decimal expected) =>
        Assert.Equal(expected, AmountParser.ApplyDirection(amount, direction));

    [Fact]
    public void TextCleaner_CollapsesWhitespaceAndRemovesBidiMarks() =>
        Assert.Equal("משכורת חודש", TextCleaner.Normalize("  ‏משכורת \t\n חודש‬ "));
}
