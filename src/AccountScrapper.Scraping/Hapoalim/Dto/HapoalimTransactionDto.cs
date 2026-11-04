namespace AccountScrapper.Scraping.Hapoalim.Dto;

internal sealed class HapoalimTransactionsResponseDto
{
    public List<HapoalimTransactionDto>? Transactions { get; set; }
}

/// <summary>A row from the current-account transactions endpoint. Only the fields we use are mapped.</summary>
internal sealed class HapoalimTransactionDto
{
    /// <summary>Transaction date as a yyyyMMdd number, e.g. 20261003.</summary>
    public long EventDate { get; set; }

    /// <summary>Absolute amount; the direction is in <see cref="EventActivityTypeCode"/>.</summary>
    public decimal EventAmount { get; set; }

    /// <summary>1 = credit (money in), 2 = debit (money out).</summary>
    public int EventActivityTypeCode { get; set; }

    public string? ActivityDescription { get; set; }

    /// <summary>0 for pending (not yet posted) entries.</summary>
    public int? SerialNumber { get; set; }

    public bool IsPending => SerialNumber == 0;
}
