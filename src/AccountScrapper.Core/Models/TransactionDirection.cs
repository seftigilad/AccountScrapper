namespace AccountScrapper.Core.Models;

public enum TransactionDirection
{
    /// <summary>The sign of the amount text itself is authoritative.</summary>
    Unknown = 0,

    /// <summary>Money entering the account (amount will be positive).</summary>
    Credit,

    /// <summary>Money leaving the account (amount will be negative).</summary>
    Debit,
}
