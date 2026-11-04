using AccountScrapper.Core.Models;

namespace AccountScrapper.Core.Abstractions;

/// <summary>
/// Data collection: logs into a site, navigates to the transactions and extracts them.
/// One implementation per bank / credit-card site.
/// </summary>
public interface ITransactionSource
{
    /// <summary>Human readable name of the source, for logs.</summary>
    string Name { get; }

    Task<IReadOnlyList<RawTransaction>> FetchTransactionsAsync(
        BankCredentials credentials,
        DateRange period,
        CancellationToken cancellationToken = default);
}
