using System.Globalization;
using AccountScrapper.Core.Models;
using AccountScrapper.Scraping.Hapoalim.Dto;

namespace AccountScrapper.Scraping.Hapoalim;

/// <summary>
/// Translates Hapoalim's wire format into the source-agnostic <see cref="RawTransaction"/>.
/// It only re-shapes data; interpreting formats is the processing layer's job.
/// </summary>
internal static class HapoalimTransactionMapper
{
    private const int CreditCode = 1;
    private const int DebitCode = 2;

    public static RawTransaction ToRaw(HapoalimTransactionDto dto) => new(
        Date: dto.EventDate.ToString(CultureInfo.InvariantCulture),
        Amount: dto.EventAmount.ToString(CultureInfo.InvariantCulture),
        Description: dto.ActivityDescription ?? string.Empty,
        Direction: dto.EventActivityTypeCode switch
        {
            CreditCode => TransactionDirection.Credit,
            DebitCode => TransactionDirection.Debit,
            _ => TransactionDirection.Unknown,
        });
}
