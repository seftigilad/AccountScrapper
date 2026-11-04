using AccountScrapper.Core.Exceptions;
using AccountScrapper.Core.Models;
using AccountScrapper.Processing;
using AccountScrapper.Processing.Masking;
using Microsoft.Extensions.Options;

namespace AccountScrapper.Tests.Processing;

public class TransactionProcessorTests
{
    private static readonly DateRange October = new(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

    private static TransactionProcessor Create(MaskingMode mode = MaskingMode.None) =>
        new(new DescriptionMasker(Options.Create(new MaskingOptions { Mode = mode })));

    [Fact]
    public void Process_NormalizesMixedFormatsIntoUniformModel()
    {
        RawTransaction[] raw =
        [
            new("20261005", "250.00", " משכורת ", TransactionDirection.Credit),
            new("03/10/2026", "1,200.50", "שכר דירה", TransactionDirection.Debit),
            new("07.10.26", "₪ -45.90", "סופר"),
        ];

        var result = Create().Process(raw, October);

        Assert.Equal(
            [
                new Transaction(new DateOnly(2026, 10, 3), -1200.50m, "שכר דירה"),
                new Transaction(new DateOnly(2026, 10, 5), 250.00m, "משכורת"),
                new Transaction(new DateOnly(2026, 10, 7), -45.90m, "סופר"),
            ],
            result);
    }

    [Fact]
    public void Process_DropsTransactionsOutsideThePeriod()
    {
        RawTransaction[] raw =
        [
            new("30/09/2026", "1", "before"),
            new("15/10/2026", "2", "inside"),
            new("01/11/2026", "3", "after"),
        ];

        var result = Create().Process(raw, October);

        Assert.Equal("inside", Assert.Single(result).Description);
    }

    [Fact]
    public void Process_AppliesMasking()
    {
        var result = Create(MaskingMode.Full).Process([new("15/10/2026", "10", "ישראל ישראלי")], October);

        Assert.Equal(DescriptionMasker.FullMaskPlaceholder, Assert.Single(result).Description);
    }

    [Fact]
    public void Process_FailsOnUnreadableRow_WithoutLeakingTheValue()
    {
        var ex = Assert.Throws<TransactionFormatException>(() =>
            Create().Process([new("15/10/2026", "10", "ok"), new("15/10/2026", "SECRET-AMOUNT", "bad")], October));

        Assert.Contains("#1", ex.Message);
        Assert.DoesNotContain("SECRET-AMOUNT", ex.Message);
    }
}
