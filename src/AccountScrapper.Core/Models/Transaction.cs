namespace AccountScrapper.Core.Models;

/// <summary>A normalized, export-ready transaction.</summary>
/// <param name="Date">Transaction date.</param>
/// <param name="Amount">Signed amount: positive for credits, negative for debits.</param>
/// <param name="Description">Description, already masked according to the configured privacy policy.</param>
public sealed record Transaction(DateOnly Date, decimal Amount, string Description);
