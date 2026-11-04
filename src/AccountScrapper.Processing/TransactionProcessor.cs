using AccountScrapper.Core.Abstractions;
using AccountScrapper.Core.Exceptions;
using AccountScrapper.Core.Models;
using AccountScrapper.Processing.Masking;
using AccountScrapper.Processing.Parsing;

namespace AccountScrapper.Processing;

/// <inheritdoc />
public sealed class TransactionProcessor : ITransactionProcessor
{
    private readonly IDescriptionMasker _masker;

    public TransactionProcessor(IDescriptionMasker masker)
    {
        _masker = masker;
    }

    public IReadOnlyList<Transaction> Process(IEnumerable<RawTransaction> rawTransactions, DateRange period)
    {
        ArgumentNullException.ThrowIfNull(rawTransactions);
        ArgumentNullException.ThrowIfNull(period);

        return rawTransactions
            .Select(Normalize)
            .Where(transaction => period.Contains(transaction.Date))
            .OrderBy(transaction => transaction.Date)
            .ToList();
    }

    private Transaction Normalize(RawTransaction raw, int index)
    {
        // A row we can't read is a hard error: silently dropping a financial record is worse than failing.
        if (!DateParser.TryParse(raw.Date, out var date))
        {
            throw new TransactionFormatException($"Transaction #{index}: unrecognized date format.");
        }

        if (!AmountParser.TryParse(raw.Amount, out var amount))
        {
            throw new TransactionFormatException($"Transaction #{index}: unrecognized amount format.");
        }

        var signedAmount = AmountParser.ApplyDirection(amount, raw.Direction);
        var description = _masker.Mask(TextCleaner.Normalize(raw.Description));

        return new Transaction(date, signedAmount, description);
    }
}
