using AccountScrapper.Core.Models;

namespace AccountScrapper.Export.Json;

/// <summary>
/// The on-disk JSON contract. Kept separate from the domain models so the file format
/// can evolve (or be versioned) without touching the rest of the application.
/// </summary>
internal sealed record JsonTransactionDocument(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateTimeOffset GeneratedAt,
    int TransactionCount,
    IReadOnlyList<JsonTransactionDocument.Item> Transactions)
{
    public static JsonTransactionDocument From(TransactionReport report) => new(
        report.Period.Start,
        report.Period.End,
        report.GeneratedAt,
        report.Transactions.Count,
        report.Transactions.Select(t => new Item(t.Date, t.Amount, t.Description)).ToList());

    internal sealed record Item(DateOnly Date, decimal Amount, string Description);
}
