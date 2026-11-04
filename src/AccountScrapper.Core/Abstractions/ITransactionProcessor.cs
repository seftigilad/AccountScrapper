using AccountScrapper.Core.Models;

namespace AccountScrapper.Core.Abstractions;

/// <summary>
/// Data processing: converts raw, source-specific rows into the uniform <see cref="Transaction"/> model,
/// handling the different date/amount formats and masking sensitive text.
/// </summary>
public interface ITransactionProcessor
{
    /// <param name="rawTransactions">Rows as collected by an <see cref="ITransactionSource"/>.</param>
    /// <param name="period">Only transactions dated inside this period are kept.</param>
    /// <returns>Normalized transactions, ordered by date.</returns>
    IReadOnlyList<Transaction> Process(IEnumerable<RawTransaction> rawTransactions, DateRange period);
}
