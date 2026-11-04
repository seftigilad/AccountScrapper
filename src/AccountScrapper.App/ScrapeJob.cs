using AccountScrapper.Core.Abstractions;
using AccountScrapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace AccountScrapper.App;

public sealed record ScrapeResult(string OutputLocation, int TransactionCount, DateRange Period);

/// <summary>
/// Orchestrates one run: collect → process → save. Holds no logic of its own beyond the order of the steps,
/// so each step can be replaced (another bank, another output format) without touching the others.
/// </summary>
public sealed class ScrapeJob
{
    private readonly ICredentialsProvider _credentialsProvider;
    private readonly ITransactionSource _source;
    private readonly ITransactionProcessor _processor;
    private readonly ITransactionExporter _exporter;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ScrapeJob> _logger;

    public ScrapeJob(
        ICredentialsProvider credentialsProvider,
        ITransactionSource source,
        ITransactionProcessor processor,
        ITransactionExporter exporter,
        TimeProvider timeProvider,
        ILogger<ScrapeJob> logger)
    {
        _credentialsProvider = credentialsProvider;
        _source = source;
        _processor = processor;
        _exporter = exporter;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ScrapeResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        var period = DateRange.PreviousMonth(today);
        _logger.LogInformation("Collecting {Source} transactions for {Period}", _source.Name, period);

        var credentials = _credentialsProvider.GetCredentials();
        var rawTransactions = await _source.FetchTransactionsAsync(credentials, period, cancellationToken);

        var transactions = _processor.Process(rawTransactions, period);
        var report = new TransactionReport(period, transactions, _timeProvider.GetUtcNow());

        var location = await _exporter.ExportAsync(report, cancellationToken);
        return new ScrapeResult(location, transactions.Count, period);
    }
}
