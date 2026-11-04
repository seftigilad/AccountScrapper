namespace AccountScrapper.Core.Models;

/// <summary>The unit handed to an exporter: the processed transactions plus context about them.</summary>
public sealed record TransactionReport(
    DateRange Period,
    IReadOnlyList<Transaction> Transactions,
    DateTimeOffset GeneratedAt);
