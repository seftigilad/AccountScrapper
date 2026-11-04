namespace AccountScrapper.Scraping.Hapoalim.Dto;

/// <summary>An entry from /ServerServices/general/accounts. Only the fields we use are mapped.</summary>
internal sealed class HapoalimAccountDto
{
    public int BankNumber { get; set; }

    public int BranchNumber { get; set; }

    public long AccountNumber { get; set; }

    /// <summary>0 (or absent) for an open account.</summary>
    public int? AccountClosingReasonCode { get; set; }

    public bool IsOpen => AccountClosingReasonCode is null or 0;

    /// <summary>The identifier the transactions endpoint expects: "bank-branch-account".</summary>
    public string Id => $"{BankNumber}-{BranchNumber}-{AccountNumber}";

    /// <summary>Safe for logs: only the last 3 digits of the account number, e.g. "12-***-***901".</summary>
    public string MaskedId
    {
        get
        {
            var number = AccountNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var visible = number.Length > 3 ? number[^3..] : number;
            return $"{BankNumber}-***-{new string('*', number.Length - visible.Length)}{visible}";
        }
    }
}
