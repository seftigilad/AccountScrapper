namespace AccountScrapper.Core.Models;

/// <summary>
/// A transaction exactly as it was collected from the source site, before any normalization.
/// Values are kept as text so that every source (DOM table, JSON API, CSV download...) can
/// produce the same shape, and all format handling lives in one place - the processing layer.
/// </summary>
/// <param name="Date">Date text as presented by the source (e.g. "20261003", "03/10/2026").</param>
/// <param name="Amount">Amount text as presented by the source (e.g. "1,234.50", "₪ -80.00", "120.00-").</param>
/// <param name="Description">Free-text description of the transaction.</param>
/// <param name="Direction">Credit/debit indication, when the source reports it separately from the amount sign.</param>
public sealed record RawTransaction(
    string Date,
    string Amount,
    string Description,
    TransactionDirection Direction = TransactionDirection.Unknown);
