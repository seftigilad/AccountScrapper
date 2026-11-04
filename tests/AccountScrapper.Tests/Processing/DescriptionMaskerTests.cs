using AccountScrapper.Processing.Masking;
using Microsoft.Extensions.Options;

namespace AccountScrapper.Tests.Processing;

public class DescriptionMaskerTests
{
    private static DescriptionMasker Create(MaskingMode mode) =>
        new(Options.Create(new MaskingOptions { Mode = mode }));

    [Fact]
    public void Partial_KeepsFirstLetterOfEachWordAndHidesAllDigits() =>
        Assert.Equal("ה**** ל***** ****-******", Create(MaskingMode.Partial).Mask("העברה לישראל 1234-567890"));

    [Fact]
    public void Partial_WorksForEnglishText() =>
        Assert.Equal("S********* P*******", Create(MaskingMode.Partial).Mask("Superpharm Purchase"));

    [Theory]
    [InlineData("אלקטרה בע\"מ", "א***** ב***")]
    [InlineData("זיכוי מאוצה\"ח", "ז**** מ******")]
    [InlineData("זיכוי מאוצה״ח", "ז**** מ******")]
    [InlineData("\"ציטוט\"", "\"צ****\"")] // quotes around a word still act as boundaries
    public void Partial_HebrewAbbreviationQuotesDoNotRevealTheNextLetter(string text, string expected) =>
        Assert.Equal(expected, Create(MaskingMode.Partial).Mask(text));

    [Fact]
    public void Partial_NeverRevealsADigit()
    {
        var masked = Create(MaskingMode.Partial).Mask("כרטיס 4580123412341234 אסמכתא 99");

        Assert.DoesNotContain(masked, char.IsDigit);
    }

    [Fact]
    public void Full_ReplacesWholeText() =>
        Assert.Equal(DescriptionMasker.FullMaskPlaceholder, Create(MaskingMode.Full).Mask("משכורת"));

    [Fact]
    public void None_ReturnsTextUnchanged() =>
        Assert.Equal("משכורת", Create(MaskingMode.None).Mask("משכורת"));

    [Theory]
    [InlineData(MaskingMode.Partial)]
    [InlineData(MaskingMode.Full)]
    public void EmptyDescription_StaysEmpty(MaskingMode mode) =>
        Assert.Equal(string.Empty, Create(mode).Mask(string.Empty));
}
