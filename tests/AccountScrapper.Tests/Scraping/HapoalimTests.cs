using AccountScrapper.Core.Models;
using AccountScrapper.Scraping.Hapoalim;
using AccountScrapper.Scraping.Hapoalim.Dto;

namespace AccountScrapper.Tests.Scraping;

public class HapoalimTests
{
    private const string Base = "https://login.bankhapoalim.co.il";

    [Theory]
    [InlineData(Base + "/ng-portals/rb/he/homepage", "Success")]
    [InlineData(Base + "/ng-portals-bt/rb/he/homepage", "Success")]
    [InlineData(Base + "/portalserver/HomePage", "Success")]
    [InlineData(Base + "/AUTHENTICATE/LOGON?flow=AUTHENTICATE&state=LOGON&errorcode=1.6&callme=false", "InvalidCredentials")]
    [InlineData(Base + "/MCP/START?flow=MCP&state=START&expiredDate=null", "PasswordChangeRequired")]
    [InlineData(Base + "/ABOUTTOEXPIRE/START?flow=ABOUTTOEXPIRE", "PasswordChangeRequired")]
    [InlineData(Base + "/cgi-bin/poalwwwc?reqName=getLogonPage", "Pending")]
    [InlineData(Base + "/ng-portals/auth/he/otp", "Pending")]
    public void ClassifyLoginUrl_RecognizesEachOutcome(string url, string expected) =>
        Assert.Equal(Enum.Parse<LoginOutcome>(expected), HapoalimSite.ClassifyLoginUrl(url));

    [Fact]
    public void TransactionsUrl_ContainsAccountAndDateRange()
    {
        var url = HapoalimSite.TransactionsUrl(Base, "ServerServices", "12-345-678", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.StartsWith(Base + "/ServerServices/current-account/transactions?", url);
        Assert.Contains("accountId=12-345-678", url);
        Assert.Contains("retrievalStartDate=20261001", url);
        Assert.Contains("retrievalEndDate=20261031", url);
    }

    [Theory]
    [InlineData(1, TransactionDirection.Credit)]
    [InlineData(2, TransactionDirection.Debit)]
    [InlineData(9, TransactionDirection.Unknown)]
    public void Mapper_TranslatesActivityCodeToDirection(int code, TransactionDirection expected)
    {
        var raw = HapoalimTransactionMapper.ToRaw(new HapoalimTransactionDto
        {
            EventDate = 20261003,
            EventAmount = 1234.5m,
            EventActivityTypeCode = code,
            ActivityDescription = "משכורת",
        });

        Assert.Equal(new RawTransaction("20261003", "1234.5", "משכורת", expected), raw);
    }

    [Fact]
    public void AccountDto_BuildsIdInTheFormatTheApiExpects() =>
        Assert.Equal("12-345-678901", new HapoalimAccountDto { BankNumber = 12, BranchNumber = 345, AccountNumber = 678901 }.Id);

    [Fact]
    public void AccountDto_MaskedId_RevealsOnlyLastThreeDigits() =>
        Assert.Equal("12-***-***901", new HapoalimAccountDto { BankNumber = 12, BranchNumber = 345, AccountNumber = 678901 }.MaskedId);
}
